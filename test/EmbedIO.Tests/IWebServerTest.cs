using EmbedIO.Sessions;
using EmbedIO.Tests.TestObjects;
using NUnit.Framework;
using System.Threading.Tasks;
using EmbedIO.Testing;
using Swan.Formatters;

namespace EmbedIO.Tests
{
    public class IWebServerTest
    {
        [Test]
        public void SetupInMemoryWebServer_ReturnsValidInstance()
        {
            using var webserver = new TestWebServer();
            NUnit.Framework.Legacy.ClassicAssert.IsNotNull(webserver);
        }

        [Test]
        public void AddModule_ReturnsValidInstance()
        {
            using var webserver = new TestWebServer();
            webserver.WithCors();

            NUnit.Framework.Legacy.ClassicAssert.AreEqual(1, webserver.Modules.Count);
        }

        [Test]
        public void SetSessionManager_ReturnsValidInstance()
        {
            using var webserver = new TestWebServer { SessionManager = new LocalSessionManager() };

            NUnit.Framework.Legacy.ClassicAssert.NotNull(webserver.SessionManager);
        }

        [Test]
        public void SetSessionManagerToNull_ReturnsValidInstance()
        {
            using var webserver = new TestWebServer();
            webserver.SessionManager = new LocalSessionManager();
            webserver.SessionManager = null;

            NUnit.Framework.Legacy.ClassicAssert.IsNull(webserver.SessionManager);
        }

        [Test]
        public async Task RunsServerAndRequestData_ReturnsValidData()
        {
            using var server = new TestWebServer();
            server
                .OnAny(ctx => ctx.SendDataAsync(new Person { Name = nameof(Person) }))
                .Start();

            var data = await server.Client.GetStringAsync("/").ConfigureAwait(false);
            NUnit.Framework.Legacy.ClassicAssert.IsNotNull(data);

            Person person = Json.Deserialize<Person>(data);
            NUnit.Framework.Legacy.ClassicAssert.IsNotNull(person);

            NUnit.Framework.Legacy.ClassicAssert.AreEqual(person.Name, nameof(Person));
        }
    }
}