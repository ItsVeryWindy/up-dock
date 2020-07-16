using System.Text.Json.Serialization;

namespace DockerUpgrader.Registry
{
    public class DockerRepositoryResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("tags")]
        public string[] Tags { get; set; }
    }
}
