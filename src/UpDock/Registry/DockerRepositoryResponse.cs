using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UpDock.Registry
{
    public class DockerRepositoryResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("tags")]
        public IReadOnlyCollection<string> Tags { get; }

        public DockerRepositoryResponse(string name, IReadOnlyCollection<string> tags)
        {
            Name = name;
            Tags = tags;
        }
    }
}
