using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;


namespace TestApp
{
    class Json
    {
        public static bool JsonValidation(string jsonstring, out JToken json)
        {
            try
            {
                var obj = JToken.Parse(jsonstring);
                json = obj;
            }
            catch (JsonReaderException jex)
            {
                TestContext.WriteLine(jex.Message);
                json = null;
                return false;
            }
            return true;
        }

        public static bool JsonSchemeValidation(string strschema, JToken json,out IList<string> errors)
        {
            bool isValid;
            using (var file = File.OpenText(strschema))
            using (var reader = new JsonTextReader(file))
            {
                var resolver = new JSchemaUrlResolver();
                var schema = JSchema.Load(reader, new JSchemaReaderSettings
                {
                    Resolver = resolver,
                    BaseUri = new Uri(Directory.GetCurrentDirectory() + "\\" + strschema)
                });
                isValid = json.IsValid(schema, out errors);
            }
            return isValid;
        }

    }
}
