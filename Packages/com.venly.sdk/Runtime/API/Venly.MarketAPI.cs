using Newtonsoft.Json.Linq;
using VenlySDK.Core;
using VenlySDK.Models;
using VenlySDK.Utils;

namespace VenlySDK
{
    public static partial class Venly
    {
        public static class MarketAPI
        {
            private static readonly eVyApiEndpoint _apiEndpoint = eVyApiEndpoint.Market;

            public static class Client
            {
               
            }

#if (UNITY_EDITOR || UNITY_SERVER || ENABLE_VENLY_API_SERVER) && !ENABLE_VENLY_PLAYFAB
            public static class Server
            {
               
            }
#endif
        }
    }
}
