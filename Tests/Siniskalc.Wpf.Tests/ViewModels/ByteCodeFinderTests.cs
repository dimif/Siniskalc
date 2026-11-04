using System.Windows;
using Siniskalc.Wpf.ViewModels;

namespace Siniskalc.Wpf.Tests.ViewModels
{
    /// <summary>
    /// Tests the Code column and the search hint that the finders of a catalog with byte codes share.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ByteCodeFinderTests
    {
        [Test]
        public void TheCodeColumnIsMutedTextWithATooltipNamingTheThing()
        {
            var column = ByteCodeFinder.CodeColumn("Code", "artifact");

            Assert.That(column.Header, Is.EqualTo("Code"));
            Assert.That(column.Path, Is.EqualTo("Code"));
            Assert.That(column.Width, Is.EqualTo(new GridLength(56)));
            Assert.That(column.Kind, Is.EqualTo(FinderColumnKind.MutedText));
            Assert.That(column.ToolTip, Is.EqualTo("The byte that stands for the artifact in a savefile, in hex"));
        }

        [Test]
        public void TheCodeColumnComesBeforeTheOtherColumns()
        {
            FinderColumn name = new("Name", "Name", new GridLength(1, GridUnitType.Star));

            var columns = ByteCodeFinder.WithCodeColumn([name], "Code", "creature");

            Assert.That(columns, Has.Count.EqualTo(2));
            Assert.That(columns[0].Path, Is.EqualTo("Code"));
            Assert.That(columns[1], Is.EqualTo(name));
        }

        [Test]
        public void TheHintMentionsTheByteCodeOnlyWhenTheFinderShowsCodes()
        {
            Assert.That(ByteCodeFinder.Hint("Type a name", true), Is.EqualTo("Type a name, or a byte code (2A, 0x2A or 42)"));
            Assert.That(ByteCodeFinder.Hint("Type a name", false), Is.EqualTo("Type a name"));
        }
    }
}
