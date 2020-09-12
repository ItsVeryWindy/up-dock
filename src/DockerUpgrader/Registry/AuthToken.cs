using System;
using System.Text.Json.Serialization;

namespace DockerUpgrader
{
    public class AuthToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = null!;

        [JsonPropertyName("token")]
        public string Token { get; set; } = null!;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("issued_at")]
        public DateTimeOffset IssuedAt { get; set; }

        public bool Expired(DateTimeOffset now) => IssuedAt.AddSeconds(ExpiresIn) <= now;
    }
}