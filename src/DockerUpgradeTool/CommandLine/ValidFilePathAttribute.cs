using System.ComponentModel.DataAnnotations;
using DockerUpgradeTool.Files;
using Microsoft.Extensions.DependencyInjection;

namespace DockerUpgradeTool.CommandLine
{
    public class ValidFilePathAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var str = (string)value;

            var provider = validationContext.GetRequiredService<IFileProvider>();

            return provider.GetFile(str)?.Exists == true
                ? ValidationResult.Success
                : new ValidationResult($"Could not find the file specified in the {validationContext.DisplayName} field.");
        }
    }
}
