using System;

namespace DockerUpgradeTool
{
    public class AuthenticationOptions
    {
        public string Username { get; }
        public string Password { get; }

        public AuthenticationOptions(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public static (string repo, AuthenticationOptions options) Parse(string authentication)
        {
            var repoSplit = authentication.Split("=", 2);

            if (repoSplit.Length < 2)
                throw new FormatException("Missing equals in authentication");

            var (repo, str) = (repoSplit[0], repoSplit[1]);

            var strSplit = str.Split(',', 2);

            if (repoSplit.Length < 2)
                throw new FormatException("Missing authentication separator");

            return (repo, new AuthenticationOptions(strSplit[0], strSplit[1]));
        }
    }
}
