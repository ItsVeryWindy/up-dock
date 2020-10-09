using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DockerUpgradeTool.CommandLine
{
    public class CommandLineBinder : ICommandLineBinder
    {
        public void Bind<T>(IReadOnlyList<CommandLineArgument> arguments, T options)
        {
            foreach (var property in typeof(T).GetProperties())
            {
                if (typeof(IEnumerable<object>).IsAssignableFrom(property.PropertyType))
                {
                    BindEnumerable(arguments, options, property);
                }
                else
                {
                    BindSingle(arguments, options, property);
                }
            }
        }

        private void BindSingle<T>(IReadOnlyList<CommandLineArgument> arguments, T options, PropertyInfo property)
        {
            var argument = arguments.FirstOrDefault(x => x.Property == property);

            if (argument == null)
                return;

            property.SetValue(options, argument.Value);
        }

        private void BindEnumerable<T>(IReadOnlyList<CommandLineArgument> arguments, T options, PropertyInfo property)
        {
            var propertyArguments = arguments.Where(x => x.Property == property).ToList();

            if(property.PropertyType.IsArray)
            {
                var array = Array.CreateInstance(property.PropertyType.GetElementType()!, propertyArguments.Count);

                for(var i = 0; i < array.Length; i++)
                {
                    array.SetValue(propertyArguments[i].Value, i);
                }

                property.SetValue(options, array);
            }
        }
    }
}
