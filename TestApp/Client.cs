using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using OAuth;
using System.Web;
using QueryParameter = System.Collections.Generic.KeyValuePair<string, string>;

namespace TestApp
{
    public struct AccesToken
    {
        [XmlElement("Consumer_Key")]
        public String ConsumerKey;
        [XmlElement("Consumer_Secret")]
        public String ConsumerSecret;
        [XmlElement("Token")]
        public String Token;
        [XmlElement("Secret")]
        public String Secret;
    }
   
    public class Client
    {
        [XmlElement("AccesToken")]
        private AccesToken m_token;
        protected OAuthBase OAuthBase;
        protected string UnreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        public Client(string xmlPath = "config.xml")
        {
            var deserializer = new XmlSerializer(typeof(AccesToken));
            TextReader reader = new StreamReader(xmlPath);
            object obj = deserializer.Deserialize(reader);
            m_token = (AccesToken)obj;
            reader.Close();
            OAuthBase = new OAuthBase();
        }

        public Client(string consumerKey, string consumerSecret, string secret = "", string token = "")
        {
            m_token = new AccesToken
            {
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                Secret = secret,
                Token = token
            };
            OAuthBase = new OAuthBase();
        }
        
        public String Get(Uri uri, List<QueryParameter> data)
        {
            string nonce = OAuthBase.GenerateNonce();
            string timeStamp = OAuthBase.GenerateTimeStamp();
            string parameters;
            string normalizedUrl;
            String paramString = "";
            if (data.Count != 0)
            {
                paramString = data.Aggregate(paramString, (current, p) => current + (UrlEncode(p.Key) + "=" + UrlEncode(p.Value) + "&"));
                paramString = paramString.Remove(paramString.Length - 1);
            }
            Uri signingUri = (paramString != String.Empty) ? new Uri(uri.ToString() + "?" + paramString) : new Uri(uri.ToString());


            string signature = OAuthBase.GenerateSignature(signingUri, m_token.ConsumerKey, m_token.ConsumerSecret,
                m_token.Token, m_token.Secret, "GET", timeStamp, nonce, OAuthBase.SignatureTypes.HMACSHA1,
                out normalizedUrl, out parameters);

            signature = HttpUtility.UrlEncode(signature);

            var requestUri = new StringBuilder(uri.ToString());
            requestUri.Append("?");
            requestUri.Append(paramString);
            requestUri.Append("&");
            requestUri.AppendFormat("oauth_consumer_key={0}&", m_token.ConsumerKey);
            requestUri.AppendFormat("oauth_nonce={0}&", nonce);
            requestUri.AppendFormat("oauth_timestamp={0}&", timeStamp);
            requestUri.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
            requestUri.AppendFormat("oauth_version={0}&", "1.0");
            requestUri.AppendFormat("oauth_signature={0}", signature);
            if (m_token.Token != String.Empty)
                requestUri.AppendFormat("&oauth_token={0}", m_token.Token);
            
            
            var request = (HttpWebRequest)WebRequest.Create(requestUri.ToString());
            
            request.Method = "GET";
            
            var response = request.GetResponse();
            var respString = new StreamReader(stream: response.GetResponseStream()).ReadToEnd();
            return respString;
        }
        protected string UrlEncode(string value)
        {
            var result = new StringBuilder();

            foreach (char symbol in value)
            {
                if (UnreservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
                }
            }

            return result.ToString();
        }
    }
}
