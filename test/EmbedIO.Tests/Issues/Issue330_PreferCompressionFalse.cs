using EmbedIO.Utilities;

using NUnit.Framework;

namespace EmbedIO.Tests.Issues
{
    public class Issue330_PreferCompressionFalse
    {
        [Test]
        public void QValueList_TryNegotiateContentEncoding_WhenPreferCompressionFalse_OnNoCompressionSpecified_ReturnsTrue()
        {
            var list = new QValueList(true, "gzip, deflate");
            NUnit.Framework.Legacy.ClassicAssert.IsTrue(list.TryNegotiateContentEncoding(false, out _, out _));
        }

        [Test]
        public void QValueList_TryNegotiateContentEncoding_WhenPreferCompressionFalse_OnNoCompressionSpecified_YieldsNone()
        {
            var list = new QValueList(true, "gzip, deflate");
            list.TryNegotiateContentEncoding(false, out CompressionMethod compressionMethod, out _);
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(CompressionMethod.None, compressionMethod);
        }

        [Test]
        public void QValueList_TryNegotiateContentEncoding_WhenPreferCompressionFalse_OnNoCompressionSpecified_YieldsIdentity()
        {
            var list = new QValueList(true, "gzip, deflate");
            list.TryNegotiateContentEncoding(false, out _, out var name);
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(CompressionMethodNames.None, name);
        }
    }
}