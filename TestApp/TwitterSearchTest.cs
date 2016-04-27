using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace TestApp
{
    [TestFixture]
    public class TwitterSearchTest
    {
        protected Uri BaseUrl = new Uri(@"https://api.twitter.com/1.1/search/tweets.json");
        protected Client Client;

        [TestFixtureSetUp]
        public void SetUp()
        {
            Client = new Client();
        }

        [TestCase("nhl")]
        [Category("positive")]
        public void SimpleQueryTest(string q)
        {
            var parametrs = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("q", q)};
            string response = String.Empty;
            try
            {
                response = Client.Get(BaseUrl, parametrs);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
            }
            JToken json;
            if (Json.JsonValidation(response, out json))
            {
                IList<string> errors;
                Json.JsonSchemeValidation("Search.json", json, out errors);
                foreach (var error in errors)
                {
                    Console.Out.WriteLine(error);
                }
            }
            else
            {
                Assert.Fail("Not valid Json");
            }
            var statuses = (JArray) json["statuses"];
            foreach (var status in statuses)
            {
                var text = status.ToString();
                if (!(text.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    Console.Out.WriteLine(text);
                }
            }
        }

        [TestCase("NHL")]
        [Category("positive")]
        public void HashTagTest(string q)
        {
            var parametrs = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("q", "#" + q)};
            string response = String.Empty;
            try
            {
                response = Client.Get(BaseUrl, parametrs);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
            }
            JToken json;
            if (Json.JsonValidation(response, out json))
            {
                IList<string> errors;
                Json.JsonSchemeValidation("Search.json", json, out errors);
                foreach (var error in errors)
                {
                    Console.Out.WriteLine(error);
                }
            }
            else
            {
                Assert.Fail("Not valid Json");
            }
            var statuses = (JArray) json["statuses"];
            foreach (var status in statuses)
            {
                var text = status["entities"]["hashtags"].ToString();
                if (!(text.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    Console.Out.WriteLine(text);
                }
            } 
        }

        [Test]
        [Category("Negative")]
        public void BadAuthorized()
        {
            var parametrs = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("q", "sample")};
            string response = String.Empty;
            var client = new Client("sdafasDFas","sdfsadfsad","asdsadfsdf","sdf2344edfas");
            try
            {
                response = client.Get(BaseUrl, parametrs);
            }
            catch (WebException exception)
            {
                if (exception.Status == WebExceptionStatus.ProtocolError)
                {
                    var exResponse = exception.Response as HttpWebResponse;
                    if (exResponse != null)
                    {
                        if ((int) exResponse.StatusCode == 401) return;
                    }
                }
            }
            Assert.Fail(response);
        }
        [Test]
        [Category("Negative")]
        public void EmptyQuery()
        {
            var parametrs = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("q", "")};
            string response = String.Empty;
            try
            {
                response = Client.Get(BaseUrl, parametrs);
            }
            catch (WebException exception)
            {
                if (exception.Status == WebExceptionStatus.ProtocolError)
                {
                    var exResponse = exception.Response as HttpWebResponse;
                    if (exResponse != null)
                    {
                        if ((int)exResponse.StatusCode == 400) return;
                    }
                }
            }
            Assert.Fail(response);
        }
        
        [Test]
        [Category("special")]
        public void SimpleQueryTest()
        {
            var parametrs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("q", "@myoffice_ru")
            };
            string response = String.Empty;
            try
            {
                response = Client.Get(BaseUrl, parametrs);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
            }
            JToken json;
            if (Json.JsonValidation(response, out json))
            {
                IList<string> errors;
                Json.JsonSchemeValidation("Search.json", json, out errors);
                foreach (var error in errors)
                {
                    Console.Out.WriteLine(error);
                }
            }
            else
            {
                Assert.Fail("Not valid Json");
            }
            var statuses = (JArray)json["statuses"];
            int i = statuses.Select(status => status.ToString()).Count(text => (text.IndexOf("LilianPertenava", StringComparison.OrdinalIgnoreCase) >= 0));
            Assert.That(i,Is.AtLeast(5));
        }
    }
}
