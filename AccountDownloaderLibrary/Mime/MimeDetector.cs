using MimeDetective;
using MimeDetective.Definitions;
using MimeDetective.Storage;
using System.Collections.Immutable;

namespace AccountDownloaderLibrary.Mime
{
    public class MimeDetector
    {
        private static readonly MimeDetector instance = new MimeDetector();

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

            var all = AllBuildier.MoveToImmutable();

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

        private string ChooseMostLikely(ImmutableArray<MimeDetective.Engine.FileExtensionMatch> results)
        {
            return results.First().Extension;
        }

    }
}
