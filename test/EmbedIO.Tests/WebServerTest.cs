﻿using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EmbedIO.Actions;
using EmbedIO.Tests.TestObjects;
using EmbedIO.WebApi;
using Swan;
using Swan.Formatters;
using NUnit.Framework;

namespace EmbedIO.Tests
{
    [TestFixture]
    public class WebServerTest
    {
        private const int Port = 88;
        private const string Prefix = "http://localhost:9696";

        private static string[] GetMultiplePrefixes()
            =>["http://localhost:9696", "http://localhost:9697", "http://localhost:9698"];

        public class Constructors : WebServerTest
        {
            [Test]
            public void DefaultConstructor()
            {
                var instance = new WebServer();
                NUnit.Framework.Legacy.ClassicAssert.IsNotNull(instance.Listener, "It has a HttpListener");
            }

            [Test]
            public void ConstructorWithPort()
            {
                var instance = new WebServer(Port);
                NUnit.Framework.Legacy.ClassicAssert.IsNotNull(instance.Listener, "It has a HttpListener");
            }

            [Test]
            public void ConstructorWithSinglePrefix()
            {
                var instance = new WebServer(Prefix);
                NUnit.Framework.Legacy.ClassicAssert.IsNotNull(instance.Listener, "It has a HttpListener");
            }

            [Test]
            public void ConstructorWithMultiplePrefixes()
            {
                var instance = new WebServer(GetMultiplePrefixes());
                NUnit.Framework.Legacy.ClassicAssert.IsNotNull(instance.Listener, "It has a HttpListener");
                NUnit.Framework.Legacy.ClassicAssert.AreEqual(3, instance.Listener.Prefixes.Count);
            }
        }

        public class TaskCancellation : WebServerTest
        {
            [Test]
            public void WithCancellationRequested_ExitsSuccessfully()
            {
                var instance = new WebServer("http://localhost:9696");

                var cts = new CancellationTokenSource();
                Task task = instance.RunAsync(cts.Token);
                cts.Cancel();

                task.Await();
                instance.Dispose();

                NUnit.Framework.Legacy.ClassicAssert.IsTrue(task.IsCompleted);
            }
        }

        public class Modules : WebServerTest
        {
            [Test]
            public void RegisterModule()
            {
                var instance = new WebServer();
                instance.Modules.Add(nameof(WebApiModule), new WebApiModule("/"));

                NUnit.Framework.Legacy.ClassicAssert.AreEqual(instance.Modules.Count, 1, "It has one module");
            }
        }

        public class General : WebServerTest
        {
            [Test]
            public void ExceptionText()
            {
                Assert.ThrowsAsync<HttpRequestException>(async () =>
                {
                    var url = Resources.GetServerAddress();

                    using var instance = new WebServer(url);
                    instance.Modules.Add(nameof(ActionModule), new ActionModule(_ => throw new InvalidOperationException("Error")));

                    _ = instance.RunAsync();
                    var request = new HttpClient();
                    await request.GetStringAsync(url);
                });
            }

            [Test]
            public void EmptyModules_NotFoundStatusCode()
            {
                Assert.ThrowsAsync<HttpRequestException>(async () =>
                {
                    var url = Resources.GetServerAddress();

                    using var instance = new WebServer(url);
                    _ = instance.RunAsync();
                    var request = new HttpClient();
                    await request.GetStringAsync(url);
                });
            }

            [TestCase("iso-8859-1")]
            [TestCase("utf-8")]
            [TestCase("utf-16")]
            public async Task EncodingTest(string encodeName)
            {
                var url = Resources.GetServerAddress();

                using var instance = new WebServer(url);
                instance.OnPost(ctx =>
                {
                    var encoding = Encoding.GetEncoding("UTF-8");

                    try
                    {
                        var encodeValue =
                            ctx.Request.ContentType.Split(';')
                                .FirstOrDefault(x =>
                                    x.Trim().StartsWith("charset", StringComparison.OrdinalIgnoreCase))
                                ?
                                .Split('=')
                                .Skip(1)
                                .FirstOrDefault()?
                                .Trim();
                        encoding = Encoding.GetEncoding(encodeValue ?? throw new InvalidOperationException());
                    }
                    catch
                    {
                        Assert.Inconclusive("Invalid encoding in system");
                    }

                    return ctx.SendDataAsync(new EncodeCheck
                    {
                        Encoding = encoding.EncodingName,
                        IsValid = ctx.Request.ContentEncoding.EncodingName == encoding.EncodingName,
                    });
                });

                _ = instance.RunAsync();

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Accept
                    .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(MimeType.Json));

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(
                        "POST DATA",
                        Encoding.GetEncoding(encodeName),
                        MimeType.Json),
                };

                using HttpResponseMessage response = await client.SendAsync(request);
                var data = await response.Content.ReadAsStringAsync();
                NUnit.Framework.Legacy.ClassicAssert.IsNotNull(data, "Data is not empty");
                EncodeCheck model = Json.Deserialize<EncodeCheck>(data);

                NUnit.Framework.Legacy.ClassicAssert.IsNotNull(model);
                NUnit.Framework.Legacy.ClassicAssert.IsTrue(model.IsValid);
            }

            internal class EncodeCheck
            {
                public string Encoding { get; set; }

                public bool IsValid { get; set; }
            }
        }
    }
}