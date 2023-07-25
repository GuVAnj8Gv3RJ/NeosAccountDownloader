using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AccountDownloader.Models;
public class ContributorsFile
{
    [JsonPropertyName("contributors")]
    public List<Contributor> Contributors { get; set; }

    public ContributorsFile(List<Contributor> contributors)
    {
        Contributors = contributors;
    }
}

public class Contributor
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("contributions")]
    public List<string> Contributions { get; set; }

    [JsonPropertyName("avatar_url")]
    public string Avatar { get; set; }

    public Contributor(string name, List<string> contributions, string avatar)
    {
        Name = name;
        Contributions = contributions;
        Avatar = avatar;
    }
}
