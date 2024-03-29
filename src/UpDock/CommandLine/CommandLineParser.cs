﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UpDock.CommandLine
{
    public class CommandLineParser : ICommandLineParser
    {
        public IReadOnlyList<CommandLineArgument> Parse<T>(string[] args, TextReader input)
        {
            var shortcutMappings = typeof(T)
                .GetProperties()
                .SelectMany(x => x.GetCustomAttributes<ShortcutAttribute>().SelectMany(y => GetShortcuts(y.Shortcuts)).Select(y => new { property = x, shortcut = y }))
                .ToDictionary(x => x.shortcut.shortcut, x => new { x.property, x.shortcut.stdin });

            var arguments = new List<CommandLineArgument>();

            for(var i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if(!shortcutMappings.TryGetValue(arg, out var info))
                {
                    arguments.Add(new CommandLineArgument(arg, null, null, null, arguments.Count(x => x.Argument == arg))
                    {
                        Errors =
                        {
                            "Argument does not exist"
                        }
                    });

                    continue;
                }

                var property = info.property;
                var stdin = info.stdin;

                if (property.PropertyType == typeof(bool))
                {
                    arguments.Add(new CommandLineArgument(arg, null, true, property, arguments.Count(x => x.Property == property)));
                    
                    continue;
                }

                var errors = new List<string>();

                string? originalValue = null;
                object? value = null;

                if (stdin)
                {
                    if (input.Peek() != -1)
                    {
                        originalValue = input.ReadLine();
                    }
                }
                else
                {
                    if (i + 1 < args.Length)
                    {
                        originalValue = args[++i];
                    }
                }

                if (originalValue is null)
                {
                    errors.Add("Value not specified");
                }
                else
                {
                    try
                    {
                        value = GetConvertedValue(property, originalValue);
                    }
                    catch (ArgumentException ex) when (ex.InnerException is FormatException)
                    {
                        errors.Add(ex.InnerException.Message);
                    }
                    catch (FormatException ex)
                    {
                        errors.Add(ex.Message);
                    }
                }

                var argument = new CommandLineArgument(arg, originalValue, value, property, arguments.Count(x => x.Property == property));

                argument.Errors.AddRange(errors);

                arguments.Add(argument);
            }

            var requiredArguments = typeof(T)
                .GetProperties()
                .Where(x => x.GetCustomAttribute<RequiredAttribute>() is not null && arguments.All(y => y.Property != x))
                .Select(x => new CommandLineArgument(Formatter.FormatShortcut(x), null, null, x, 0));

            arguments.AddRange(requiredArguments);

            return arguments;
        }

        private static object GetConvertedValue(PropertyInfo property, string value)
        {
            var converter = GetConverter(property);

            return converter.ConvertFromString(value);
        }

        private static TypeConverter GetConverter(PropertyInfo property)
        {
            var converterName = property.GetCustomAttribute<TypeConverterAttribute>()?.ConverterTypeName;

            var type = converterName is null ? GetCorrectTypeToConvert(property.PropertyType) : Type.GetType(converterName);

            if (converterName is null)
                return TypeDescriptor.GetConverter(type);

            return (TypeConverter)Activator.CreateInstance(type!)!;
        }

        private static Type GetCorrectTypeToConvert(Type type)
        {
            if (!typeof(IEnumerable<object>).IsAssignableFrom(type))
                return type;
            
            if (type.IsArray)
                return type.GetElementType()!;

            if (type.IsGenericType)
                return type.GetGenericArguments()[0];

            return type;
        }

        private static IEnumerable<(string shortcut, bool stdin)> GetShortcuts(IReadOnlyCollection<Shortcut> shortcuts)
        {
            foreach(var shortcut in shortcuts)
            {
                yield return (shortcut.ToString(), false);
                yield return (shortcut.ToString(true), true);
            }
        }
    }
}
