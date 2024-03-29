﻿using System;
using System.Text.Json.Serialization;

namespace UpDock
{
    public class AuthToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("issued_at")]
        public DateTimeOffset IssuedAt { get; set; }

        public AuthToken(string accessToken, string token)
        {
            AccessToken = accessToken;
            Token = token;
        }

        public bool Expired(DateTimeOffset now) => IssuedAt.AddSeconds(ExpiresIn) <= now;
    }
}
