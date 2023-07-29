using MimeDetective.Storage;
using System.Collections.Immutable;

namespace AccountDownloaderLibrary.Mime
{
    public class CustomTypes
    {
        // Based on: https://github.com/MediatedCommunications/Mime-Detective/blob/main/src/MimeDetective/Definitions/Default/FileTypes/Default.FileTypes.Audio.cs
        public static ImmutableArray<Definition> MESHX()
        {
            return new List<Definition>() {
                new() {
                    File = new() {
                        Extensions = new[]{"meshx"}.ToImmutableArray(),
                        MimeType = "application/meshx"
                    },
                    Signature = new Segment[] {
                        PrefixSegment.Create(0, "05 4D 65 73 68 58"),
                    }.ToSignature(),
                },
            }.ToImmutableArray();
        }

        public static ImmutableArray<Definition> ANIMX() =>
        new List<Definition>() {
            new() {
                File = new() {
                    Extensions = new[]{"animx"}.ToImmutableArray(),
                    MimeType = "application/octet-stream"
                },
                Signature = new Segment[] {
                    PrefixSegment.Create(0, "05 41 6E 69 6D 58"),
                }.ToSignature()
            },
        }.ToImmutableArray();
    }
}
