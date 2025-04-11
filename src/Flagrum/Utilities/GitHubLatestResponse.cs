using System.Text.Json.Serialization;

namespace Flagrum.Utilities;

public class GitHubLatestResponse
{
    [JsonPropertyName("tag_name")] public string TagName { get; set; } = null!;
}