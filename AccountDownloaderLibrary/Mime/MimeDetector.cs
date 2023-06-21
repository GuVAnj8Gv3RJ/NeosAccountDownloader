using MimeDetective;
using MimeDetective.Definitions;
using MimeDetective.Storage;
using System.Collections.Immutable;

namespace AccountDownloaderLibrary.Mime
{
    public class MimeDetector
    {
        public static readonly MimeDetector Instance = new MimeDetector();

        private ContentInspector inspector = null;

        private MimeDetector()
        {
            ImmutableArray<Definition> exhaustiveDefs = new ExhaustiveBuilder()
            {
                UsageType = MimeDetective.Definitions.Licensing.UsageType.PersonalNonCommercial
            }.Build();

            ImmutableArray<Definition>.Builder AllBuildier = ImmutableArray.CreateBuilder<Definition>();
            AllBuildier.AddRange(exhaustiveDefs);
            AllBuildier.AddRange(CustomTypes.MESHX());

            var all = AllBuildier.ToImmutable();

            inspector = new ContentInspectorBuilder()
            {
                Definitions = all
            }.Build();
        }

        public string MostLikelyFileExtension(string filePath)
        {
            return ChooseMostLikely(inspector.Inspect(filePath).ByFileExtension());
        }

        public string MostLikelyFileExtension(FileStream stream)
        {
            return ChooseMostLikely(inspector.Inspect(stream).ByFileExtension());
        }

        public string MostLikelyFileExtension(Stream stream)
        {
            return ChooseMostLikely(inspector.Inspect(stream).ByFileExtension());
        }

        private string ChooseMostLikely(ImmutableArray<MimeDetective.Engine.FileExtensionMatch> results)
        {
            return results.First().Extension;
        }

    }
}
