﻿using MimeDetective;
using MimeDetective.Definitions;
using MimeDetective.Engine;
using MimeDetective.Storage;
using System.Collections.Immutable;
//TODO: nullables
#nullable enable
namespace AccountDownloaderLibrary.Mime;

public class MimeDetector
{
    public static readonly MimeDetector Instance = new MimeDetector();

    private ContentInspector Inspector { get; }

    private MimeTypeToFileExtensionLookup MimeTypeToFileExtensionLookup { get; }

    //TODO: DI
    private MimeDetector()
    {
        ///trimmed: https://github.com/MediatedCommunications/Mime-Detective#1--trim-the-data-you-dont-need
        var exhaustiveDefs = new ExhaustiveBuilder()
        {
            UsageType = MimeDetective.Definitions.Licensing.UsageType.PersonalNonCommercial
        }.Build()
        .TrimDescription()
        .TrimMeta()
        .TrimCategories();

        //TODO: do we need exhaustive?
        ImmutableArray<Definition>.Builder AllBuildier = ImmutableArray.CreateBuilder<Definition>();
        AllBuildier.AddRange(exhaustiveDefs);
        AllBuildier.AddRange(CustomTypes.CustomNeosFiles());
        AllBuildier.AddRange(StrangeTypes.BrokenTypes());

        var all = AllBuildier.ToImmutable();

        Inspector = new ContentInspectorBuilder()
        {
            Definitions = all,
            Parallel = true,
        }.Build();

        MimeTypeToFileExtensionLookup = new MimeTypeToFileExtensionLookupBuilder()
        {
            Definitions = all
        }.Build();
    }

    // Gets the first value
    public string? ExtensionFromMime(string mime)
    {
        return MimeTypeToFileExtensionLookup.TryGetValue(mime);
    }

    public IEnumerable<FileExtensionMatch>? PossibleExtensions(string mime)
    {
        return MimeTypeToFileExtensionLookup.TryGetValues(mime);
    }

    public string? MostLikelyFileExtension(string filePath)
    {
        return ChooseMostLikely(Inspector.Inspect(filePath).ByFileExtension());
    }

    public string? MostLikelyFileExtension(FileStream stream)
    {
        return ChooseMostLikely(Inspector.Inspect(stream).ByFileExtension());
    }

    public string? MostLikelyFileExtension(Stream stream)
    {
        return ChooseMostLikely(Inspector.Inspect(stream).ByFileExtension());
    }

    private string? ChooseMostLikely(ImmutableArray<MimeDetective.Engine.FileExtensionMatch> results)
    {
        if (results.Count() > 0)
            return results.First().Extension;
        return null;
    }

}
