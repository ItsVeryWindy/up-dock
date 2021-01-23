using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace UpDock.CommandLine
{
    public class CommandLineValidator : ICommandLineValidator
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandLineValidator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Validate<T>(IReadOnlyList<CommandLineArgument> arguments)
        {
            foreach(var property in typeof(T).GetProperties())
            {
                if (typeof(IEnumerable<object>).IsAssignableFrom(property.PropertyType))
                {
                    ValidateEnumerable<T>(arguments, property);
                }
                else
                {
                    ValidateSingle<T>(arguments, property);
                }
            }
        }

        private void ValidateSingle<T>(IReadOnlyList<CommandLineArgument> arguments, PropertyInfo property)
        {
            var propertyArguments = arguments.Where(x => x.Property == property).ToList();

            if (propertyArguments.Count > 1)
            {
                propertyArguments[0].Errors.Add("This cannot be specified more than once");
            }

            var argument = propertyArguments.FirstOrDefault();

            if (argument == null)
                return;

            var validationAttributes = property.GetCustomAttributes<ValidationAttribute>();

            var context = new ValidationContext(arguments, _serviceProvider, null)
            {
                DisplayName = argument.Argument,
                MemberName = property.Name,
            };

            foreach (var validationAttribute in validationAttributes)
            {
                var result = validationAttribute.GetValidationResult(argument.Value, context);

                if (result == ValidationResult.Success)
                    continue;

                argument.Errors.Add(result.ErrorMessage);
            }
        }

        private void ValidateEnumerable<T>(IReadOnlyList<CommandLineArgument> arguments, PropertyInfo property)
        {
            var propertyArguments = arguments.Where(x => x.Property == property).ToList();

            var validationAttributes = property.GetCustomAttributes<ValidationAttribute>();

            foreach(var argument in propertyArguments)
            {
                var context = new ValidationContext(arguments, _serviceProvider, null)
                {
                    DisplayName = argument.Argument,
                    MemberName = property.Name,
                };

                foreach (var validationAttribute in validationAttributes)
                {
                    var result = validationAttribute.GetValidationResult(argument.Value, context);

                    if (result == ValidationResult.Success)
                        continue;

                    argument.Errors.Add(result.ErrorMessage);
                }
            }
        }
    }
}
