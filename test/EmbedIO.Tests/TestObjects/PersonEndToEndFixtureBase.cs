using System.Threading.Tasks;

using Swan.Formatters;

namespace EmbedIO.Tests.TestObjects
{
    public abstract class PersonEndToEndFixtureBase(bool useTestWebServer) : EndToEndFixtureBase(useTestWebServer)
    {
        protected async Task ValidatePersonAsync(string url)
        {
            Person current = PeopleRepository.Database[0];

            var jsonBody = await Client.GetStringAsync(url);

            NUnit.Framework.Legacy.ClassicAssert.IsNotNull(jsonBody, "Json Body is not null");
            NUnit.Framework.Legacy.ClassicAssert.IsNotEmpty(jsonBody, "Json Body is not empty");

            Person item = Json.Deserialize<Person>(jsonBody);

            NUnit.Framework.Legacy.ClassicAssert.IsNotNull(item, "Json Object is not null");
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(item.Name, current.Name, "Remote objects equality");
        }
    }
}