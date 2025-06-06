﻿using NUnit.Framework;
using System.Threading.Tasks;
using EmbedIO.Tests.TestObjects;

namespace EmbedIO.Tests
{
    [TestFixture]
    public class RegexRoutingTest : EndToEndFixtureBase
    {
        protected override void OnSetUp()
        {
            Server.WithModule(new TestRegexModule("/"));
        }

        public class GetData : RegexRoutingTest
        {
            [Test]
            public async Task GetDataWithoutRegex()
            {
                var call = await Client.GetStringAsync("empty");

                NUnit.Framework.Legacy.ClassicAssert.AreEqual(string.Empty, call);
            }

            [Test]
            public async Task GetDataWithRegex()
            {
                var call = await Client.GetStringAsync("data/1");

                NUnit.Framework.Legacy.ClassicAssert.AreEqual("1", call);
            }

            [Test]
            public async Task GetDataWithMultipleRegex()
            {
                var call = await Client.GetStringAsync("data/1/2");

                NUnit.Framework.Legacy.ClassicAssert.AreEqual("2", call);
            }
        }
    }
}