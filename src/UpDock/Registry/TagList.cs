using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UpDock.Registry
{
    public class TagList
    {
        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("tags")]
        public IReadOnlyCollection<string> Tags { get; }

        public TagList(string name, IReadOnlyCollection<string> tags)
        {
            Name = name;
            Tags = tags;
        }
    }
}
