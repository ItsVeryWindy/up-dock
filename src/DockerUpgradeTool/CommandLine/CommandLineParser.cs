using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace DockerUpgradeTool.CommandLine
{
    public class CommandLineParser : ICommandLineParser
    {
        public IReadOnlyList<CommandLineArgument> Parse<T>(string[] args)
        {
            var shortcutMappings = typeof(T)
                .GetProperties()
                .SelectMany(x => x.GetCustomAttributes<ShortcutAttribute>().SelectMany(y => y.Shortcuts).Select(y => new { property = x, shortcut = y }))
                .ToDictionary(x => x.shortcut, x => x.property);

            var arguments = new List<CommandLineArgument>();

            for(var i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if(!shortcutMappings.TryGetValue(arg, out var property))
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

                if (property.PropertyType == typeof(bool))
                {
                    arguments.Add(new CommandLineArgument(arg, null, true, property, arguments.Count(x => x.Property == property)));
                    
                    continue;
                }

                var errors = new List<string>();

                string? originalValue = null;
                object? value = null;

                if (args.Length == i + 1)
                {
                    errors.Add("Value not specified");
                }
                else
                {
                    originalValue = args[i + 1];

                    try
                    {
                        value = GetConvertedValue(property, originalValue);
                    }
                    catch(ArgumentException ex) when (ex.InnerException is FormatException)
                    {
                        errors.Add(ex.InnerException.Message);
                    }
                    catch(FormatException ex)
                    {
                        errors.Add(ex.Message);
                    }
                }

                var argument = new CommandLineArgument(arg, originalValue, value, property, arguments.Count(x => x.Property == property));

                argument.Errors.AddRange(errors);

                arguments.Add(argument);

                i++;
            }

            var requiredArguments = typeof(T)
                .GetProperties()
                .Where(x => x.GetCustomAttribute<RequiredAttribute>() != null && arguments.All(y => y.Property != x))
                .Select(x => new CommandLineArgument(Formatter.FormatShortcut(x), null, null, x, 0));

            arguments.AddRange(requiredArguments);

            return arguments;
        }

        private object GetConvertedValue(PropertyInfo property, string value)
        {
            var converter = GetConverter(property);

            return converter.ConvertFromString(value);
        }

        private TypeConverter GetConverter(PropertyInfo property)
        {
            var converterName = property.GetCustomAttribute<TypeConverterAttribute>()?.ConverterTypeName;

            var type = converterName == null ? GetCorrectTypeToConvert(property.PropertyType) : Type.GetType(converterName);

            if (converterName == null)
                return TypeDescriptor.GetConverter(type);

            return (TypeConverter)Activator.CreateInstance(type!)!;
        }

        private Type GetCorrectTypeToConvert(Type type)
        {
            if (!typeof(IEnumerable<object>).IsAssignableFrom(type))
                return type;
            
            if (type.IsArray)
                return type.GetElementType()!;

            if (type.IsGenericType)
                return type.GetGenericArguments()[0];

            return type;
        }
    }
}
