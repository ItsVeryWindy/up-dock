using System.Security.Cryptography;
using System.Text;

namespace UpDock
{
    public static class HashAlgorithmExtensions
    {
        private static readonly Encoding Encoding = new UTF8Encoding(false);

        public static string ComputeHash(this HashAlgorithm algorithm, string str)
        {
            var bytes = algorithm.ComputeHash(Encoding.GetBytes(str));

            var builder = new StringBuilder();

            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
