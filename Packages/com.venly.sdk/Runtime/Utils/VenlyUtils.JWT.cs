﻿//SOURCE: https://github.com/monry/JWT-for-Unity

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VenlySDK.Models;
using VenlySDK.Models.Shared;

namespace VenlySDK.Utils
{
    public class SignatureVerificationException : Exception
    {
        public SignatureVerificationException(string message)
            : base(message)
        {
        }
    }

    public class VyJwtInfo
    {
        public bool HasNftAccess { get; internal set; }
        public bool HasWalletAccess { get; internal set; }
        public bool HasMarketAccess { get; internal set; }
        public eVyEnvironment Environment { get; internal set; }
    }

    public static partial class VenlyUtils
    {
        #region JsonSerializer
        /// <summary>
        /// Provides JSON Serialize and Deserialize.  Allows custom serializers used.
        /// </summary>
        internal interface IJsonSerializer
        {
            /// <summary>
            /// Serialize an object to JSON string
            /// </summary>
            /// <param name="obj">object</param>
            /// <returns>JSON string</returns>
            string Serialize(object obj);

            /// <summary>
            /// Deserialize a JSON string to typed object.
            /// </summary>
            /// <typeparam name="T">type of object</typeparam>
            /// <param name="json">JSON string</param>
            /// <returns>typed object</returns>
            T Deserialize<T>(string json);
        }

        internal class DefaultJsonSerializer : IJsonSerializer
        {
            public string Serialize(object obj)
            {
                return JsonConvert.SerializeObject(obj);
            }

            public T Deserialize<T>(string json)
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
        }
        #endregion

        public static class JWT
        {
            #region VyAccessToken Parsing

            public static void UpdateBearerToken(VyAccessTokenDto bearer)
            {
                bearer.JwtInfo = ExtractInfoFromToken(bearer);
            }

            public static VyJwtInfo ExtractInfoFromToken(VyAccessTokenDto bearer)
            {
                var jwtInfo = new VyJwtInfo();

                //Decode Token
                var payloadJson = Decode(bearer.Token, string.Empty, false);
                var json = JObject.Parse(payloadJson);

                //Environment (default to staging if iss is not found...)
                var iss = json["iss"];
                jwtInfo.Environment = iss?.ToString().Contains("staging", StringComparison.OrdinalIgnoreCase)??true
                    ? eVyEnvironment.staging
                    : eVyEnvironment.production;

                //Realm Access
                var realms = json.SelectToken("realm_access.roles")?.ToObject<string[]>();
                jwtInfo.HasNftAccess = realms?.Any(r => r.Equals("NFT-API"))??false;
                jwtInfo.HasMarketAccess = realms?.Any(r => r.Equals("Market API"))??false;

                //todo: change once JWT realm-access info has changed to also incorporate Wallet Realm
                jwtInfo.HasWalletAccess = json["scope"]?.ToString().Contains("sign:wallets", StringComparison.OrdinalIgnoreCase)??false;

                return jwtInfo;
            }
            #endregion

            #region JWT Decoder
            public enum JwtHashAlgorithm
            {
                HS256,
                HS384,
                HS512
            }

            /// <summary>
            /// Pluggable JSON Serializer
            /// </summary>
            internal static IJsonSerializer JsonSerializer = new DefaultJsonSerializer();
            private static readonly IDictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>> HashAlgorithms;
            private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            static JWT()
            {
                HashAlgorithms = new Dictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>>
                {
                    { JwtHashAlgorithm.HS256, (key, value) => { using (var sha = new HMACSHA256(key)) { return sha.ComputeHash(value); } } },
                    { JwtHashAlgorithm.HS384, (key, value) => { using (var sha = new HMACSHA384(key)) { return sha.ComputeHash(value); } } },
                    { JwtHashAlgorithm.HS512, (key, value) => { using (var sha = new HMACSHA512(key)) { return sha.ComputeHash(value); } } }
                };
            }

            /// <summary>
            /// Given a JWT, decode it and return the JSON payload.
            /// </summary>
            /// <param name="token">The JWT.</param>
            /// <param name="key">The key bytes that were used to sign the JWT.</param>
            /// <param name="verify">Whether to verify the signature (default is true).</param>
            /// <returns>A string containing the JSON payload.</returns>
            /// <exception cref="SignatureVerificationException">Thrown if the verify parameter was true and the signature was NOT valid or if the JWT was signed with an unsupported algorithm.</exception>
            public static string Decode(string token, byte[] key, bool verify = true)
            {
                var parts = token.Split('.');
                if (parts.Length != 3)
                {
                    throw new ArgumentException("Token must consist from 3 delimited by dot parts");
                }
                var header = parts[0];
                var payload = parts[1];
                var crypto = Base64UrlDecode(parts[2]);

                var headerJson = Encoding.UTF8.GetString(Base64UrlDecode(header));
                var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));

                var headerData = JsonSerializer.Deserialize<Dictionary<string, object>>(headerJson);

                if (verify)
                {
                    var bytesToSign = Encoding.UTF8.GetBytes(string.Concat(header, ".", payload));
                    var algorithm = (string)headerData["alg"];

                    var signature = HashAlgorithms[GetHashAlgorithm(algorithm)](key, bytesToSign);
                    var decodedCrypto = Convert.ToBase64String(crypto);
                    var decodedSignature = Convert.ToBase64String(signature);

                    Verify(decodedCrypto, decodedSignature, payloadJson);
                }

                return payloadJson;
            }

            private static void Verify(string decodedCrypto, string decodedSignature, string payloadJson)
            {
                if (decodedCrypto != decodedSignature)
                {
                    throw new SignatureVerificationException(string.Format("Invalid signature. Expected {0} got {1}", decodedCrypto, decodedSignature));
                }

                // verify exp claim https://tools.ietf.org/html/draft-ietf-oauth-json-web-token-32#section-4.1.4
                var payloadData = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson);
                if (payloadData.ContainsKey("exp") && payloadData["exp"] != null)
                {
                    // safely unpack a boxed int 
                    int exp;
                    try
                    {
                        exp = Convert.ToInt32(payloadData["exp"]);
                    }
                    catch (Exception)
                    {
                        throw new SignatureVerificationException("Claim 'exp' must be an integer.");
                    }

                    var secondsSinceEpoch = Math.Round((DateTime.UtcNow - UnixEpoch).TotalSeconds);
                    if (secondsSinceEpoch >= exp)
                    {
                        throw new SignatureVerificationException("Token has expired.");
                    }
                }
            }

            /// <summary>
            /// Given a JWT, decode it and return the JSON payload.
            /// </summary>
            /// <param name="token">The JWT.</param>
            /// <param name="key">The key that was used to sign the JWT.</param>
            /// <param name="verify">Whether to verify the signature (default is true).</param>
            /// <returns>A string containing the JSON payload.</returns>
            /// <exception cref="SignatureVerificationException">Thrown if the verify parameter was true and the signature was NOT valid or if the JWT was signed with an unsupported algorithm.</exception>
            public static string Decode(string token, string key, bool verify = true)
            {
                return Decode(token, Encoding.UTF8.GetBytes(key), verify);
            }

            /// <summary>
            /// Given a JWT, decode it and return the payload as an object (by deserializing it with <see cref="System.Web.Script.Serialization.JavaScriptSerializer"/>).
            /// </summary>
            /// <param name="token">The JWT.</param>
            /// <param name="key">The key that was used to sign the JWT.</param>
            /// <param name="verify">Whether to verify the signature (default is true).</param>
            /// <returns>An object representing the payload.</returns>
            /// <exception cref="SignatureVerificationException">Thrown if the verify parameter was true and the signature was NOT valid or if the JWT was signed with an unsupported algorithm.</exception>
            public static object DecodeToObject(string token, byte[] key, bool verify = true)
            {
                var payloadJson = Decode(token, key, verify);
                var payloadData = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson);
                return payloadData;
            }

            /// <summary>
            /// Given a JWT, decode it and return the payload as an object (by deserializing it with <see cref="System.Web.Script.Serialization.JavaScriptSerializer"/>).
            /// </summary>
            /// <param name="token">The JWT.</param>
            /// <param name="key">The key that was used to sign the JWT.</param>
            /// <param name="verify">Whether to verify the signature (default is true).</param>
            /// <returns>An object representing the payload.</returns>
            /// <exception cref="SignatureVerificationException">Thrown if the verify parameter was true and the signature was NOT valid or if the JWT was signed with an unsupported algorithm.</exception>
            public static object DecodeToObject(string token, string key, bool verify = true)
            {
                return DecodeToObject(token, Encoding.UTF8.GetBytes(key), verify);
            }

            /// <summary>
            /// Given a JWT, decode it and return the payload as an object (by deserializing it with <see cref="System.Web.Script.Serialization.JavaScriptSerializer"/>).
            /// </summary>
            /// <typeparam name="T">The <see cref="Type"/> to return</typeparam>
            /// <param name="token">The JWT.</param>
            /// <param name="key">The key that was used to sign the JWT.</param>
            /// <param name="verify">Whether to verify the signature (default is true).</param>
            /// <returns>An object representing the payload.</returns>
            /// <exception cref="SignatureVerificationException">Thrown if the verify parameter was true and the signature was NOT valid or if the JWT was signed with an unsupported algorithm.</exception>
            public static T DecodeToObject<T>(string token, byte[] key, bool verify = true)
            {
                var payloadJson = Decode(token, key, verify);
                var payloadData = JsonSerializer.Deserialize<T>(payloadJson);
                return payloadData;
            }

            /// <summary>
            /// Given a JWT, decode it and return the payload as an object (by deserializing it with <see cref="System.Web.Script.Serialization.JavaScriptSerializer"/>).
            /// </summary>
            /// <typeparam name="T">The <see cref="Type"/> to return</typeparam>
            /// <param name="token">The JWT.</param>
            /// <param name="key">The key that was used to sign the JWT.</param>
            /// <param name="verify">Whether to verify the signature (default is true).</param>
            /// <returns>An object representing the payload.</returns>
            /// <exception cref="SignatureVerificationException">Thrown if the verify parameter was true and the signature was NOT valid or if the JWT was signed with an unsupported algorithm.</exception>
            public static T DecodeToObject<T>(string token, string key, bool verify = true)
            {
                return DecodeToObject<T>(token, Encoding.UTF8.GetBytes(key), verify);
            }

            private static JwtHashAlgorithm GetHashAlgorithm(string algorithm)
            {
                switch (algorithm)
                {
                    case "HS256": return JwtHashAlgorithm.HS256;
                    case "HS384": return JwtHashAlgorithm.HS384;
                    case "HS512": return JwtHashAlgorithm.HS512;
                    default: throw new SignatureVerificationException("Algorithm not supported.");
                }
            }

            // from JWT spec
            public static string Base64UrlEncode(byte[] input)
            {
                var output = Convert.ToBase64String(input);
                output = output.Split('=')[0]; // Remove any trailing '='s
                output = output.Replace('+', '-'); // 62nd char of encoding
                output = output.Replace('/', '_'); // 63rd char of encoding
                return output;
            }

            // from JWT spec
            public static byte[] Base64UrlDecode(string input)
            {
                var output = input;
                output = output.Replace('-', '+'); // 62nd char of encoding
                output = output.Replace('_', '/'); // 63rd char of encoding
                switch (output.Length % 4) // Pad with trailing '='s
                {
                    case 0: break; // No pad chars in this case
                    case 2: output += "=="; break; // Two pad chars
                    case 3: output += "="; break;  // One pad char
                    default: throw new Exception("Illegal base64url string!");
                }
                var converted = Convert.FromBase64String(output); // Standard base64 decoder
                return converted;
            }
            #endregion
        }
    }
}
