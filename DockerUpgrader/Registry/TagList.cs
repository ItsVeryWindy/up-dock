using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DockerUpgrader.Registry
{
    public class TagList
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("tags")]
        public IReadOnlyList<string> Tags { get; set; }
    }
}