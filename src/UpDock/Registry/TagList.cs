﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UpDock.Registry
{
    public class TagList
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("tags")]
        public IReadOnlyList<string> Tags { get; set; } = null!;
    }
}
