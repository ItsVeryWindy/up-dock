﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using UpDock.Files;
using Microsoft.Extensions.DependencyInjection;

namespace UpDock.CommandLine
{
    public class ValidJsonFileAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
                return ValidationResult.Success;

            var str = (string)value;

            var provider = validationContext.GetRequiredService<IFileProvider>();

            var stream = provider.GetFile(str).CreateReadStream();

            if (stream is null)
                return ValidationResult.Success;

            try
            {
                JsonDocument.Parse(stream);
            }
            catch(JsonException)
            {
                return new ValidationResult($"The file specified in the {validationContext.DisplayName} field is not valid json.");
            }

            return ValidationResult.Success;
        }
    }
}
