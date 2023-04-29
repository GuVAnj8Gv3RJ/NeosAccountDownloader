using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace AccountDownloaderLibrary
{
    public enum QuotaType
    {
        Base,
        Normal,
        Share,
        Unlimited,
    }

    public class QuotaSource
    {
        [JsonProperty(PropertyName = "id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "bytes")]
        [JsonPropertyName("bytes")]
        public long Bytes { get; set; }

        [JsonProperty(PropertyName = "expiresOn")]
        [JsonPropertyName("expiresOn")]
        public DateTime? ExpiresOn { get; set; }

        [JsonProperty(PropertyName = "activatesOn")]
        [JsonPropertyName("activatesOn")]
        public DateTime? ActivatesOn { get; set; }

        [JsonProperty(PropertyName = "createdOn")]
        [JsonPropertyName("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [JsonProperty(PropertyName = "group")]
        [JsonPropertyName("group")]
        public string Group { get; set; }

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonProperty(PropertyName = "type")]
        [JsonPropertyName("type")]
        public QuotaType Type { get; set; }

        [JsonProperty(PropertyName = "canBeReshared")]
        [JsonPropertyName("canBeReshared")]
        public bool CanBeShared { get; set; }

        [JsonProperty(PropertyName = "name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsExpired => IsExpiredAtTime(DateTime.UtcNow);

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsActive => IsActiveAtTime(DateTime.UtcNow);

        public bool IsActiveAtTime(DateTime timePoint) => !IsExpiredAtTime(timePoint) && (ActivatesOn == null || timePoint >= ActivatesOn);
        public bool IsExpiredAtTime(DateTime timePoint) => ExpiresOn.HasValue && timePoint >= ExpiresOn.Value;
    }

    public class QuotaShare
    {
        [JsonProperty(PropertyName = "targetOwnerId")]
        [JsonPropertyName("targetOwnerId")]
        public string TargetOwnerId { get; set; }

        [JsonProperty(PropertyName = "shareRatio")]
        [JsonPropertyName("shareRatio")]
        public float ShareRatio { get; set; }

        [JsonProperty(PropertyName = "maxShareBytes")]
        [JsonPropertyName("maxShareBytes")]
        public long MaxShareBytes { get; set; }

        [JsonProperty(PropertyName = "currentShareBytes")]
        [JsonPropertyName("currentShareBytes")]
        public long CurrentShareBytes { get; set; }

        [JsonProperty(PropertyName = "canBeReshared")]
        [JsonPropertyName("canBeReshared")]
        public bool CanBeReshared { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Storage
    {
        [JsonProperty(PropertyName = "id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "ownerId")]
        [JsonPropertyName("ownerId")]
        public string OwnerId { get; set; }

        [JsonProperty(PropertyName = "usedBytes")]
        [JsonPropertyName("usedBytes")]
        public long UsedBytes { get; set; }

        [JsonProperty(PropertyName = "quotaBytes")]
        [JsonPropertyName("quotaBytes")]
        public long QuotaBytes { get; set; }

        [JsonProperty(PropertyName = "fullQuotaBytes")]
        [JsonPropertyName("fullQuotaBytes")]
        public long FullQuotaBytes { get; set; }

        [JsonProperty(PropertyName = "shareableQuotaBytes")]
        [JsonPropertyName("shareableQuotaBytes")]
        public long ShareableQuotaBytes { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long SharedQuotaBytes => FullQuotaBytes - QuotaBytes;

        [JsonProperty(PropertyName = "quotaSources")]
        [JsonPropertyName("quotaSources")]
        public List<QuotaSource> QuotaSources { get; set; }

        [JsonProperty(PropertyName = "quotaShares")]
        [JsonPropertyName("quotaShares")]
        public List<QuotaShare> QuotaShares { get; set; }
    }
}

