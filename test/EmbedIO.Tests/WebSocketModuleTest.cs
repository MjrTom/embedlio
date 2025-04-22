using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EmbedIO.Tests.TestObjects;
using Swan.Formatters;
using NUnit.Framework;

namespace EmbedIO.Tests
{
    [TestFixture]
    public class WebSocketModuleTest : EndToEndFixtureBase
    {
        public WebSocketModuleTest()
            : base(false)
        {
        }

        protected override void OnSetUp()
        {
            _ = Server
                .WithModule(new TestWebSocket("/test"))
                .WithModule(new BigDataWebSocket("/bigdata"))
                .WithModule(new CloseWebSocket("/close"));
        }

        [Test]
        public async Task TestConnectWebSocket()
        {
            var websocketUrl = new Uri(WebServerUrl.Replace("http", "ws") + "test");

            var clientSocket = new System.Net.WebSockets.ClientWebSocket();
            await clientSocket.ConnectAsync(websocketUrl, default);

            NUnit.Framework.Legacy.ClassicAssert.AreEqual(
                System.Net.WebSockets.WebSocketState.Open,
                clientSocket.State,
                $"Connection should be open, but the status is {clientSocket.State} - {websocketUrl}");

            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes("HOLA"));
            await clientSocket.SendAsync(buffer, System.Net.WebSockets.WebSocketMessageType.Text, true, default);

            NUnit.Framework.Legacy.ClassicAssert.AreEqual(await ReadString(clientSocket), "HELLO");
        }

        [Test]
        public async Task TestSendBigDataWebSocket()
        {
            var webSocketUrl = new Uri($"{WebServerUrl.Replace("http", "ws")}bigdata");

            var clientSocket = new System.Net.WebSockets.ClientWebSocket();
            await clientSocket.ConnectAsync(webSocketUrl, default).ConfigureAwait(false);

            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes("HOLA"));
            await clientSocket.SendAsync(buffer, System.Net.WebSockets.WebSocketMessageType.Text, true, default).ConfigureAwait(false);

            var json = await ReadString(clientSocket).ConfigureAwait(false);
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(Json.Serialize(BigDataWebSocket.BigDataObject), json);
        }

        [Test]
        public async Task TestWithDifferentCloseResponse()
        {
            var webSocketUrl = new Uri($"{WebServerUrl.Replace("http", "ws")}close");

            var clientSocket = new System.Net.WebSockets.ClientWebSocket();
            await clientSocket.ConnectAsync(webSocketUrl, default).ConfigureAwait(false);

            var buffer = new ArraySegment<byte>(new byte[8192]);
            System.Net.WebSockets.WebSocketReceiveResult result = await clientSocket.ReceiveAsync(buffer, default).ConfigureAwait(false);

            NUnit.Framework.Legacy.ClassicAssert.IsTrue(result.CloseStatus.HasValue);
            NUnit.Framework.Legacy.ClassicAssert.IsTrue(result.CloseStatus.Value == System.Net.WebSockets.WebSocketCloseStatus.InvalidPayloadData);
        }

        protected static async Task<string> ReadString(System.Net.WebSockets.ClientWebSocket ws)
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);

            await using var ms = new MemoryStream();
            System.Net.WebSockets.WebSocketReceiveResult result;

            do
            {
                result = await ws.ReceiveAsync(buffer, default);
                ms.Write(buffer!.Array, buffer.Offset, result.Count);
            }
            while (!result.EndOfMessage);

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}