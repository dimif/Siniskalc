using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Siniskalc.Tests.Homm.Common;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    /// <summary>
    /// The rule in CLAUDE.md: view models never touch byte chunks or the byte layout of a struct. They ask the collection classes
    /// (heroes by id or name, castles, players by colour) for models.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class ViewModelLayeringTests
    {
        private static readonly Regex Forbidden = new(
            @"\b(HeroChunk|CastleChunk|PlayerChunk|BytesChunk|RawBytesChunk|CastleLayout|PlayerLayout|HeroOffsets|HeroOffsetName)\b|\.GetByteAt\(|\.GetBytesAt\(|\.Chunk\b|\.OriginalChunk\b",
            RegexOptions.Compiled);

        // The hex editor works on raw bytes by design: its edit targets take the session, not the view models.
        private static readonly string[] Allowed = [];

        private static string ViewModelsFolder([CallerFilePath] string thisFile = "")
        {
            // This file is Tests/Siniskalc.Homm3.Tests/Integration/ViewModels/ViewModelLayeringTests.cs, so the root is four folders up.
            var root = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", "..", "..", ".."));
            Assert.That(File.Exists(Path.Combine(root, "Siniskalc.sln")), Is.True, "the repository root was not found");
            return Path.Combine(root, "Siniskalc.Homm3", "ViewModels");
        }

        [Test]
        public void ViewModelsDoNotUseChunksOrTheByteLayout()
        {
            var offenders = ViewModelLayering.Offenders(ViewModelsFolder(), Forbidden, Allowed);

            Assert.That(offenders, Is.Empty, string.Join(Environment.NewLine, offenders));
        }
    }
}
