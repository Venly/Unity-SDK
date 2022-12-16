using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Venly.Utils;

namespace Venly.Models.Internal
{
    public class RequestData
    {
        public string Uri { get; private set; }
        public HttpMethod Method { get; private set; }
        public HttpContent Content { get; private set; }
        public eVyApiEndpoint Endpoint { get; private set; }
        public bool RequiredWrapping { get; private set; }

        public string SelectPropertyName { get; private set; }
        public bool MustSelectProperty => !string.IsNullOrEmpty(SelectPropertyName);

        private RequestData() { }

        #region Creation

        private static bool CheckWrapRequirement(eVyApiEndpoint endpoint)
        {
            switch (endpoint)
            {
                case eVyApiEndpoint.Nft: return false;
                case eVyApiEndpoint.Auth: return false;
                case eVyApiEndpoint.Extension: return false;
                default: return true;
            }
        }

        private static RequestData Create(HttpMethod method, string uri, eVyApiEndpoint endpoint)
        {
            return new RequestData
            {
                Method = method,
                Uri = uri,
                Endpoint = endpoint,
                RequiredWrapping = CheckWrapRequirement(endpoint)
            };
        }

        public static RequestData Get(string uri, eVyApiEndpoint endpoint)
        {
            return Create(HttpMethod.Get, uri, endpoint);
        }

        public static RequestData Patch(string uri, eVyApiEndpoint endpoint)
        {
            return Create(new HttpMethod("PATCH"), uri, endpoint);
        }

        public static RequestData Post(string uri, eVyApiEndpoint endpoint)
        {
            return Create(HttpMethod.Post, uri, endpoint);
        }

        public static RequestData Put(string uri, eVyApiEndpoint endpoint)
        {
            return Create(HttpMethod.Put, uri, endpoint);
        }

        public static RequestData Delete(string uri, eVyApiEndpoint endpoint)
        {
            return Create(HttpMethod.Delete, uri, endpoint);
        }
        #endregion

        #region Content
        public RequestData AddJsonContent<T>(T content)
        {
            Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
            return this;
        }

        public RequestData AddFormContent<T>(T content)
        {
            if (content is Dictionary<string, string> contentDictionary)
            {
                Content = new FormUrlEncodedContent(contentDictionary);
            }
            else
            {
                var data = new Dictionary<string, string>();
                foreach (var property in JObject.FromObject(content))
                {
                    if(property.Value != null)
                        data.Add(property.Key, property.Value.ToString());
                }

                Content = new FormUrlEncodedContent(data);
            }

            return this;
        }
        #endregion

        #region Misc
        public RequestData SelectProperty(string propertyName)
        {
            SelectPropertyName = propertyName;
            return this;
        }
        #endregion
    }
}
