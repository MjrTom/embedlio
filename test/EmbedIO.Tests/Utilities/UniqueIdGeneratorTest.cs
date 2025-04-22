using EmbedIO.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace EmbedIO.Tests.Utilities
{
    public class UniqueIdGeneratorTest
    {
        [Test]
        public void GetNext_ReturnsValidString()
        {
            var id = UniqueIdGenerator.GetNext();
            NUnit.Framework.Legacy.ClassicAssert.IsNotNull(id);
            NUnit.Framework.Legacy.ClassicAssert.IsNotEmpty(id);
        }

        [Test]
        public void GetNext_ReturnsUniqueId()
        {
            var ids = new string[100];
            for (var i = 0; i < ids.Length; i++)
                ids[i] = UniqueIdGenerator.GetNext();
            CollectionAssert.AllItemsAreUnique(ids);
        }
    }
}