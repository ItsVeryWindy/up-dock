using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using DockerUpgradeTool.Files;
using Microsoft.Extensions.DependencyInjection;

namespace DockerUpgradeTool.CommandLine
{
    public class ValidJsonFileAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var str = (string)value;

            var provider = validationContext.GetRequiredService<IFileProvider>();

            var stream = provider.GetFile(str)?.CreateReadStream();

            if (stream == null)
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
