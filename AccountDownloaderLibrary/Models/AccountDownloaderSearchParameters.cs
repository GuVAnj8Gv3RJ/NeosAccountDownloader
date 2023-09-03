using CloudX.Shared;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace AccountDownloaderLibrary.Models;

/**
 * CloudX will filter on OnlyFeatured defaulting it to false, even if the user hasn't specified their preference.
 * 
 * This is rather silly. So we extend the class and re-define it as a nullable. This allows it to be ignored when sent to the cloud.
 * 
 * We'll have to log this as a bug for Neos to fix in CloudX too.
 */

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class AccountDownloaderSearchParameters : SearchParameters
{
    [JsonProperty(PropertyName = "onlyFeatured", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("onlyFeatured")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingDefault)]
    public new bool? OnlyFeatured { get; set; } = null;
}
