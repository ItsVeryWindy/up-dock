using System;
using System.ComponentModel;
using System.Globalization;
using UpDock.Imaging;

namespace UpDock.CommandLine
{
    internal class DockerImageTemplatePatternConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var str = (string)value;

            return DockerImageTemplate.Parse(str).CreatePattern(true, true, true, null);
        }
    }
}
