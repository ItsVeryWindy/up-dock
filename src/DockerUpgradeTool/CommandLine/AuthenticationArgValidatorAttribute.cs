using System;
using PowerArgs;

namespace DockerUpgradeTool.CommandLine
{
    public class AuthenticationArgValidatorAttribute : ArgValidator
    {
        public override void Validate(string name, ref string arg)
        {
            try
            {
                AuthenticationOptions.Parse(arg);
            }
            catch(FormatException ex)
            {
                throw new ValidationArgException($"-{name}: {ex.Message}", ex);
            }
        }
    }
}
