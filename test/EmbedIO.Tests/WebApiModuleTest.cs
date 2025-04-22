using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using EmbedIO.Tests.TestObjects;
using EmbedIO.WebApi;

using NUnit.Framework;

using Swan.Formatters;

namespace EmbedIO.Tests
{
    [TestFixture]
    public class WebApiModuleTest : PersonEndToEndFixtureBase
    {
        public WebApiModuleTest()
            : base(true)
        {
        }

        protected override void OnSetUp()
        {
            Server.WithWebApi("/api", m => m.WithController<TestController>());
        }

        public class HttpGet : WebApiModuleTest
        {
            [Test]
            public async Task EmptyResponse_ReturnsOk()
            {
                HttpResponseMessage response = await Client.GetAsync("/api/empty");

                NUnit.Framework.Legacy.ClassicAssert.IsNotNull(response);
                NUnit.Framework.Legacy.ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
        }

        public class HttpPost : WebApiModuleTest
        {
            [Test]
            public async Task JsonData_ReturnsOk()
            {
                var model = new Person { Key = 10, Name = "Test" };
                var payloadJson = new StringContent(
                    Json.Serialize(model),
                    WebServer.DefaultEncoding,
                    MimeType.Json);

                HttpResponseMessage response = await Client.PostAsync("/api/regex", payloadJson);

                Person result = Json.Deserialize<Person>(await response.Content.ReadAsStringAsync());
                NUnit.Framework.Legacy.ClassicAssert.IsNotNull(result);
                NUnit.Framework.Legacy.ClassicAssert.AreEqual(model.Name, result.Name);
            }
        }

        public class Http405 : WebApiModuleTest
        {
            [Test]
            public async Task ValidPathInvalidMethod_Returns405()
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, "/api/regex");

                HttpResponseMessage response = await Client.SendAsync(request);

                NUnit.Framework.Legacy.ClassicAssert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
            }
        }

        public class QueryData : WebApiModuleTest
        {
            [Test]
            public async Task QueryDataAttribute_ReturnsCorrectValues()
            {
                HttpResponseMessage result = await Client.GetAsync($"/api/{TestController.QueryTestPath}?a=first&one=1&a=second&two=2&none&equal=&a[]=third");
                NUnit.Framework.Legacy.ClassicAssert.IsNotNull(result);
                var data = await result.Content.ReadAsStringAsync();
                Dictionary<string, object> dict = Json.Deserialize<Dictionary<string, object>>(data);
                NUnit.Framework.Legacy.ClassicAssert.IsNotNull(dict);

                NUnit.Framework.Legacy.ClassicAssert.AreEqual("1", dict["one"]);
                NUnit.Framework.Legacy.ClassicAssert.AreEqual("2", dict["two"]);
                NUnit.Framework.Legacy.ClassicAssert.AreEqual(string.Empty, dict["none"]);
                NUnit.Framework.Legacy.ClassicAssert.AreEqual(string.Empty, dict["equal"]);
                Assert.Throws<KeyNotFoundException>(() =>
                {
                    var three = dict["three"];
                });

                var a = dict["a"] as IEnumerable<object>;
                NUnit.Framework.Legacy.ClassicAssert.NotNull(a);
                var list = a!.Cast<string>().ToList();
                NUnit.Framework.Legacy.ClassicAssert.AreEqual(3, list.Count);
                NUnit.Framework.Legacy.ClassicAssert.AreEqual("first", list[0]);
                NUnit.Framework.Legacy.ClassicAssert.AreEqual("second", list[1]);
                NUnit.Framework.Legacy.ClassicAssert.AreEqual("third", list[2]);
            }

            [Test]
            public async Task QueryFieldAttribute_ReturnsCorrectValue()
            {
                var value = Guid.NewGuid().ToString();
                HttpResponseMessage result = await Client.GetAsync($"/api/{TestController.QueryFieldTestPath}?id={value}");
                NUnit.Framework.Legacy.ClassicAssert.IsNotNull(result);
                var returnedValue = await result.Content.ReadAsStringAsync();
                NUnit.Framework.Legacy.ClassicAssert.AreEqual(Json.Serialize(value), returnedValue);
            }
        }

        public class FormData : WebApiModuleTest
        {
            [TestCase("Id", "Id")]
            [TestCase("Id[0]", "Id[1]")]
            public async Task MultipleIndexedValues_ReturnsOk(string label1, string label2)
            {
                KeyValuePair<string, string>[] content =
                [
                    new KeyValuePair<string, string>("Test", "data"),
                    new KeyValuePair<string, string>(label1, "1"),
                    new KeyValuePair<string, string>(label2, "2"),
                ];

                var formContent = new FormUrlEncodedContent(content);

                HttpResponseMessage result = await Client.PostAsync($"/api/{TestController.EchoPath}", formContent);
                NUnit.Framework.Legacy.ClassicAssert.IsNotNull(result);
                var data = await result.Content.ReadAsStringAsync();
                FormDataSample obj = Json.Deserialize<FormDataSample>(data);
                NUnit.Framework.Legacy.ClassicAssert.IsNotNull(obj);
                NUnit.Framework.Legacy.ClassicAssert.AreEqual(content[0].Value, obj.Test);
                NUnit.Framework.Legacy.ClassicAssert.AreEqual(2, obj.Id.Count);
                NUnit.Framework.Legacy.ClassicAssert.AreEqual(content[^1].Value, obj.Id[^1]);
            }

            [Test]
            public async Task TestDictionaryFormData_ReturnsOk()
            {
                KeyValuePair<string, string>[] content =
                [
                    new KeyValuePair<string, string>("Test", "data"),
                    new KeyValuePair<string, string>("Id", "1"),
                ];

                var formContent = new FormUrlEncodedContent(content);

                HttpResponseMessage result = await Client.PostAsync("/api/" + TestController.EchoPath, formContent);

                NUnit.Framework.Legacy.ClassicAssert.IsNotNull(result);
                var data = await result.Content.ReadAsStringAsync();
                Dictionary<string, string> obj = Json.Deserialize<Dictionary<string, string>>(data);
                NUnit.Framework.Legacy.ClassicAssert.AreEqual(2, obj.Keys.Count);

                NUnit.Framework.Legacy.ClassicAssert.AreEqual(content[0].Key, obj.First().Key);
                NUnit.Framework.Legacy.ClassicAssert.AreEqual(content[0].Value, obj.First().Value);
            }
        }

        internal class FormDataSample
        {
            public string Test { get; set; }
            public List<string> Id { get; set; }
        }

        public class GetJsonData : WebApiModuleTest
        {
            [Test]
            public Task WithRegexId_ReturnsOk()
                => ValidatePersonAsync("/api/regex/1");

            [Test]
            public Task WithOptRegexIdAndValue_ReturnsOk()
                => ValidatePersonAsync("/api/regexopt/1");

            [Test]
            public async Task WithOptRegexIdAndNonValue_ReturnsOk()
            {
                var jsonBody = await Client.GetStringAsync("/api/regexopt");
                List<Person> remoteList = Json.Deserialize<List<Person>>(jsonBody);

                NUnit.Framework.Legacy.ClassicAssert.AreEqual(
                    PeopleRepository.Database.Count,
                    remoteList.Count,
                    "Remote list count equals local list");
            }

            [Test]
            public Task WithRegexDate_ReturnsOk()
            {
                Person person = PeopleRepository.Database[0];
                return ValidatePersonAsync($"/api/regexdate/{person.DoB:yyyy-MM-dd}");
            }

            [Test]
            public Task WithRegexWithTwoParams_ReturnsOk()
            {
                Person person = PeopleRepository.Database[0];
                return ValidatePersonAsync($"/api/regextwo/{person.MainSkill}/{person.Age}");
            }

            [Test]
            public Task WithRegexWithOptionalParams_ReturnsOk()
            {
                Person person = PeopleRepository.Database[0];
                return ValidatePersonAsync($"/api/regexthree/{person.MainSkill}");
            }
        }

        public class TestBaseRoute : WebApiModuleTest
        {
            [Test]
            public async Task ControllerMethodWithBaseRoute_ReturnsCorrectSubPath()
            {
                var subPath = "/" + Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture);
                var receivedSubPath = await Client.GetStringAsync("/api/testBaseRoute" + subPath);

                NUnit.Framework.Legacy.ClassicAssert.AreEqual(Json.Serialize(subPath), receivedSubPath);
            }
        }
    }
}