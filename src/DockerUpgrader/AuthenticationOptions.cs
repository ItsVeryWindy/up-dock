namespace DockerUpgrader
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
    }
}