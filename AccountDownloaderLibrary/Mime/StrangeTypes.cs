using MimeDetective.Storage;
using System.Collections.Immutable;

namespace AccountDownloaderLibrary.Mime;

public class StrangeTypes
{
    // Could this just be a C# Map, yes. But I like how we can expand types here to have additional information that may help with other issues.
    public static ImmutableArray<Definition> BrokenTypes()
    {
        return new List<Definition>() {
                new() {
                    File = new() {
                        Extensions = new[]{"ogg"}.ToImmutableArray(),
                        MimeType = "application/ogg"
                    },
                },
                new() {
                    File = new() {
                        Extensions = new[]{"tga"}.ToImmutableArray(),
                        MimeType = "image/x-targa"
                    },
                },
                new() {
                    File = new() {
                        Extensions = new[]{"otf"}.ToImmutableArray(),
                        MimeType = "font/otc"
                    },
                },
            }.ToImmutableArray();
    }
}

