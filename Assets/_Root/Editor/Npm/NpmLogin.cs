using System;
using System.Net;
using System.Text;
using UnityEngine;

namespace com.snorlax.upm
{
    [Serializable]
    internal class NpmLoginRequest
    {
        public string name;
        public string password;
    }


    public class ExpectContinueAware : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                var hwr = request as HttpWebRequest;
                hwr.ServicePoint.Expect100Continue = false;
                hwr.AllowAutoRedirect = false;
            }

            return request;
        }
    }


    public class NpmLogin
    {
        internal static string UrlCombine(string start, string more)
        {
            if (string.IsNullOrEmpty(start))
            {
                return more;
            }

            if (string.IsNullOrEmpty(more))
            {
                return start;
            }

            return start.TrimEnd('/') + "/" + more.TrimStart('/');
        }

        public static string GetBintrayToken(string user, string apiKey) { return Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + apiKey)); }

        public static NpmResponse GetLoginToken(string url, string user, string password)
        {
            using (var client = new WebClient())
            {
                string loginUri = UrlCombine(url, "/-/user/org.couchdb.user:" + user);
                client.Headers.Add(HttpRequestHeader.Accept, "application/json");
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + password)));

                var request = new NpmLoginRequest { name = user, password = password };

                string requestString = JsonUtility.ToJson(request);

                try
                {
                    string responseString = client.UploadString(loginUri, WebRequestMethods.Http.Put, requestString);
                    var response = JsonUtility.FromJson<NpmResponse>(responseString);
                    return response;
                }
                catch (WebException e)
                {
                    var response = new NpmResponse { error = WebExceptionParser.ParseWebException(e) };
                    return response;
                }
            }
        }
    }
}