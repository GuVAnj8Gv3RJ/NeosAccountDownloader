using AccountDownloader.Models;
using System.Text.Json.Serialization;

namespace AccountDownloader.Utilities;

// This makes our one case of JSON De-Serialize better/faster for AOT scenarios.
// I'm not entirely sure how this works, so here are some references for later.
// https://stackoverflow.com/questions/72335157/how-to-use-jsonserializer-deserializet-with-application-code-trimming-enabled
// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation
// https://devblogs.microsoft.com/dotnet/system-text-json-in-dotnet-7/

[JsonSerializable(typeof(ContributorsFile))]
internal partial class SourceGenerationContext : JsonSerializerContext { }
