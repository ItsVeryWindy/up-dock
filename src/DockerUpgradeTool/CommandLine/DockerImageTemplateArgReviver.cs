using System;
using DockerUpgradeTool.Imaging;
using PowerArgs;

namespace DockerUpgradeTool.CommandLine
{
    public static class DockerImageTemplateArgReviver
    {
        [ArgReviver]
        public static DockerImageTemplatePattern Revive(string key, string val)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            try
            {
                return DockerImageTemplate.Parse(val).CreatePattern(true, true, null);
            }
            catch(FormatException ex)
            {
                throw new ArgException($"-{key}: {ex.Message}");
            }
        }
    }
}
