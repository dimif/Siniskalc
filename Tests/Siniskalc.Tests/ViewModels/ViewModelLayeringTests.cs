using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Siniskalc.Core.Data;

namespace Siniskalc.Tests.ViewModels
{
    /// <summary>
    /// The rule in CLAUDE.md: view models never touch byte chunks or the byte layout of a struct. They ask the collection classes
    /// (heroes by id or name, castles, players by colour) for models.
    /// </summary>
    [TestFixture]
    internal sealed class ViewModelLayeringTests
    {
        private static readonly Regex Forbidden = new(
            @"\b(HeroChunk|CastleChunk|PlayerChunk|BytesChunk|RawBytesChunk|CastleLayout|PlayerLayout|HeroOffsets|HeroOffsetName)\b|\.GetByteAt\(|\.GetBytesAt\(|\.Chunk\b|\.OriginalChunk\b",
            RegexOptions.Compiled);

        // The hex editor works on raw bytes by design: its edit targets take the session, not the view models.
        private static readonly string[] Allowed = [];

        private static string ViewModelsFolder([CallerFilePath] string thisFile = "")
        {
            // This file is Tests/Siniskalc.Tests/ViewModels/ViewModelLayeringTests.cs, so the root is three folders up.
            var root = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", "..", ".."));
            Assert.That(File.Exists(Path.Combine(root, "Siniskalc.sln")), Is.True, "the repository root was not found");
            return Path.Combine(root, "Siniskalc", "ViewModels");
        }

        [Test]
        public void ViewModelsDoNotUseChunksOrTheByteLayout()
        {
            var offenders = new List<string>();
            foreach (var file in Directory.GetFiles(ViewModelsFolder(), "*.cs"))
            {
                if (Allowed.Contains(Path.GetFileName(file)))
                {
                    continue;
                }

                var lines = File.ReadAllLines(file);
                for (var i = 0; i < lines.Length; i++)
                {
                    var code = lines[i].TrimStart();
                    if (!code.StartsWith("//") && Forbidden.IsMatch(code))
                    {
                        offenders.Add($"{Path.GetFileName(file)}:{i + 1}: {code}");
                    }
                }
            }

            Assert.That(offenders, Is.Empty, string.Join(Environment.NewLine, offenders));
        }
    }
}
