﻿using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EmbedIO.Authentication;
using NUnit.Framework;

namespace EmbedIO.Tests
{
    [TestFixture]
    public class BasicAuthenticationModuleTest : EndToEndFixtureBase
    {
        private const string UserName = "root";
        private const string Password = "password1234";

        protected override void OnSetUp()
        {
            Server
                .WithModule(new BasicAuthenticationModule("/").WithAccount(UserName, Password))
                .OnAny(ctx =>
                {
                    ctx.Response.SetEmptyResponse((int)HttpStatusCode.OK);
                    return Task.FromResult(true);
                });
        }

        [Test]
        public async Task RequestWithValidCredentials_ReturnsOK()
        {
            HttpResponseMessage response = await MakeRequest(UserName, Password).ConfigureAwait(false);
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Status Code OK");
        }

        [Test]
        public async Task RequestWithValidCredentials_ReturnsValidWWWAuthenticateHeader()
        {
            HttpResponseMessage response = await MakeRequest(UserName, Password).ConfigureAwait(false);
            NUnit.Framework.Legacy.ClassicAssert.AreEqual("Basic realm=\"/\" charset=UTF-8", response.Headers.WwwAuthenticate.ToString());
        }

        [Test]
        public async Task RequestWithInvalidCredentials_ReturnsUnauthorized()
        {
            const string wrongPassword = "wrongpaassword";

            HttpResponseMessage response = await MakeRequest(UserName, wrongPassword).ConfigureAwait(false);
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status Code Unauthorized");
        }

        [Test]
        public async Task RequestWithInvalidCredentials_ReturnsValidWWWAuthenticateHeader()
        {
            const string wrongPassword = "wrongpaassword";

            HttpResponseMessage response = await MakeRequest(UserName, wrongPassword).ConfigureAwait(false);
            NUnit.Framework.Legacy.ClassicAssert.AreEqual("Basic realm=\"/\" charset=UTF-8", response.Headers.WwwAuthenticate.ToString());
        }

        [Test]
        public async Task RequestWithNoAuthorizationHeader_ReturnsUnauthorized()
        {
            HttpResponseMessage response = await MakeRequest(null, null).ConfigureAwait(false);
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status Code Unauthorized");
        }

        [Test]
        public async Task RequestWithNoAuthorizationHeader_ReturnsValidWWWAuthenticateHeader()
        {
            HttpResponseMessage response = await MakeRequest(null, null).ConfigureAwait(false);
            NUnit.Framework.Legacy.ClassicAssert.AreEqual("Basic realm=\"/\" charset=UTF-8", response.Headers.WwwAuthenticate.ToString());
        }

        private Task<HttpResponseMessage> MakeRequest(string? userName, string? password)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, WebServerUrl);

            if (userName == null)
                return Client.SendAsync(request);

            var encodedCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{password}"));
            var authHeaderValue = new System.Net.Http.Headers.AuthenticationHeaderValue("basic", encodedCredentials);
            request.Headers.Add("Authorization", authHeaderValue.ToString());

            return Client.SendAsync(request);
        }
    }
}