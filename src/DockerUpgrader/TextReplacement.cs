using DockerUpgrader.Files;
using DockerUpgrader.Imaging;

namespace DockerUpgrader
{
    public class TextReplacement
    {
        public string Group { get; }
        public IFileInfo File { get; }
        public string From { get; }
        public DockerImagePattern FromPattern { get; }
        public string To { get; }
        public DockerImagePattern ToPattern { get; }
        public int LineNumber { get; }
        public int Start { get; }
        
        public TextReplacement(string group, IFileInfo file, string from, DockerImagePattern fromPattern, string to, DockerImagePattern toPattern, int lineNumber, int start)
        {
            Group = group;
            File = file;
            From = from;
            FromPattern = fromPattern;
            To = to;
            ToPattern = toPattern;
            LineNumber = lineNumber;
            Start = start;
        }
    }
}