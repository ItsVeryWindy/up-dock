using System.Text.Json.Serialization;

namespace DockerUpgradeTool.Registry
{
    public class DockerRepositoryResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("tags")]
        public string[] Tags { get; set; } = null!;
    }
}
