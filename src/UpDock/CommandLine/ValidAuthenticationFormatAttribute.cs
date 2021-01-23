using System;
using System.ComponentModel.DataAnnotations;

namespace UpDock.CommandLine
{
    public class ValidAuthenticationFormatAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                AuthenticationOptions.Parse((string)value);

                return ValidationResult.Success;
            }
            catch (FormatException ex)
            {
                return new ValidationResult(ex.Message);
            }
        }
    }
}
