﻿using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using EmbedIO.Tests.TestObjects;

using NUnit.Framework;

using Swan;

namespace EmbedIO.Tests
{
    [TestFixture]
    public class HttpsTest
    {
        private const string DefaultMessage = "HOLA";
        private const string HttpsUrl = "https://localhost:5555";

        [Test]
        public void OpenWebServerHttpsWithLinuxOrMac_ThrowsInvalidOperation()
        {
            if (SwanRuntime.OS == Swan.OperatingSystem.Windows)
                Assert.Ignore("Ignore Windows");

            Assert.Throws<PlatformNotSupportedException>(() =>
            {
                WebServerOptions options = new WebServerOptions()
                    .WithUrlPrefix(HttpsUrl)
                    .WithAutoLoadCertificate();

                new WebServer(options).Void();
            });
        }

        [Test]
        public void OpenWebServerHttpsWithoutCert_ThrowsInvalidOperation()
        {
            if (SwanRuntime.OS != Swan.OperatingSystem.Windows)
                Assert.Ignore("Only Windows");

            WebServerOptions options = new WebServerOptions()
                .WithUrlPrefix(HttpsUrl)
                .WithAutoRegisterCertificate();

            Assert.Throws<InvalidOperationException>(() => new WebServer(options).Void());
        }

        [Test]
        public void OpenWebServerHttpsWithInvalidStore_ThrowsInvalidOperation()
        {
            if (SwanRuntime.OS != Swan.OperatingSystem.Windows)
                Assert.Ignore("Only Windows");

            WebServerOptions options = new WebServerOptions()
                .WithUrlPrefix(HttpsUrl)
                .WithCertificate(new X509Certificate2())
                .WithAutoRegisterCertificate();

            Assert.Throws<System.Security.Cryptography.CryptographicException>(() => _ = new WebServer(options));
        }

        // Bypass certificate validation.
        private static bool ValidateCertificate(object sender,
                                                X509Certificate certificate,
                                                X509Chain chain,
                                                SslPolicyErrors sslPolicyErrors)
            => true;
    }
}
