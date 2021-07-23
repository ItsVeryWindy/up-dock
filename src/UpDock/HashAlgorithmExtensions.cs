using System.Security.Cryptography;
using System.Text;

namespace UpDock
{
    public static class HashAlgorithmExtensions
    {
        public static string ComputeHash(this HashAlgorithm algorithm, string str)
        {
            var bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(str));

            var builder = new StringBuilder();

            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
