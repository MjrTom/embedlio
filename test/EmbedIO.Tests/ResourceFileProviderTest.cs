using System.IO;
using System.Linq;

using EmbedIO.Files;
using EmbedIO.Testing;

using NUnit.Framework;

namespace EmbedIO.Tests
{
    [TestFixture]
    public class ResourceFileProviderTest
    {
        private readonly IFileProvider _fileProvider = new ResourceFileProvider(
            typeof(TestWebServer).Assembly,
            typeof(TestWebServer).Namespace + ".Resources");

        private readonly IMimeTypeProvider _mimeTypeProvider = new MockMimeTypeProvider();

        [TestCase("/index.html", "index.html")]
        [TestCase("/sub/index.html", "index.html")]
        public void MapFile_ReturnsCorrectFileInfo(string urlPath, string name)
        {
            MappedResourceInfo? info = _fileProvider.MapUrlPath(urlPath, _mimeTypeProvider);

            NUnit.Framework.Legacy.ClassicAssert.IsNotNull(info, "info != null");
            NUnit.Framework.Legacy.ClassicAssert.IsTrue(info.IsFile, "info.IsFile == true");
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(name, info.Name, "info.Name has the correct value");
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(StockResource.GetLength(urlPath), info.Length, "info.Length has the correct value");
        }

        [TestCase("/index.html")]
        [TestCase("/sub/index.html")]
        public void OpenFile_ReturnsCorrectContent(string urlPath)
        {
            MappedResourceInfo? info = _fileProvider.MapUrlPath(urlPath, _mimeTypeProvider);
            var expectedText = StockResource.GetText(urlPath, WebServer.DefaultEncoding);

            using Stream stream = _fileProvider.OpenFile(info.Path);
            using var reader = new StreamReader(stream, WebServer.DefaultEncoding, false, WebServer.StreamCopyBufferSize, true);
            var actualText = reader.ReadToEnd();

            NUnit.Framework.Legacy.ClassicAssert.AreEqual(expectedText, actualText, "Content is the same as embedded resource");
        }

        [Test]
        public void GetDirectoryEntries_ReturnsEmptyEnumerable()
        {
            System.Collections.Generic.IEnumerable<MappedResourceInfo> entries = _fileProvider.GetDirectoryEntries(string.Empty, _mimeTypeProvider);
            NUnit.Framework.Legacy.ClassicAssert.IsFalse(entries.Any(), "There are no entries");
        }
    }
}