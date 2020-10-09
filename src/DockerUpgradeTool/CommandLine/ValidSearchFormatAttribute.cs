using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DockerUpgradeTool.CommandLine
{
    public class ValidSearchFormatAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var str = (string)value;

            var strs = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return strs.Any(x => x.StartsWith("org:") || x.StartsWith("repo:") || x.StartsWith("user:"))
                ? ValidationResult.Success
                : new ValidationResult($"The {validationContext.DisplayName} field should contain one of org, repo or user search options.");
        }
    }
}
