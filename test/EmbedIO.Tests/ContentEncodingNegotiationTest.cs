using EmbedIO.Utilities;
using NUnit.Framework;

namespace EmbedIO.Tests
{
    [TestFixture]
    public class ContentEncodingNegotiationTest
    {
        [TestCase("identity;q=1, *;q=0", true, CompressionMethod.None, CompressionMethodNames.None)]
        [TestCase("identity;q=1, *;q=0", false, CompressionMethod.None, CompressionMethodNames.None)]
        public void ContentEncodingNegotiation_Succeeds(
            string requestHeaders,
            bool preferCompression,
            CompressionMethod expectedCompressionMethod,
            string expectedCompressionMethodName)
        {
            var list = new QValueList(true, requestHeaders);
            var negotiated = list.TryNegotiateContentEncoding(preferCompression, out CompressionMethod actualCompressionMethod, out var actualCompressionMethodName);
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(true, negotiated);
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(expectedCompressionMethod, actualCompressionMethod);
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(expectedCompressionMethodName, actualCompressionMethodName);
        }

        [TestCase("*;q=0", true)]
        [TestCase("*;q=0", false)]
        public void ContentEncodingNegotiation_Fails(string requestHeaders, bool preferCompression)
        {
            var list = new QValueList(true, requestHeaders);
            var negotiated = list.TryNegotiateContentEncoding(preferCompression, out _, out _);
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(false, negotiated);
        }
    }
}