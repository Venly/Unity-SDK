using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Venly.Models;
using Venly.Models.Internal;

namespace Venly.Utils
{
    public static partial class VenlyUtils
    {
        //RequestData to HttpRequestMessage
        public static HttpRequestMessage ToRequestMessage(this RequestData data, VyAccessToken token = null)
        {
            var requestMsg = new HttpRequestMessage(data.Method, VenlyUtils.GetUrl(data.Uri, data.Endpoint, VenlySettings.Environment))
            {
                Content = data.Content
            };

            requestMsg.Headers.Add("Accept", "application/json");

            if (token != null)
            {
                requestMsg.Headers.Add("Authorization", $"Bearer {token.Token}");
            }

            return requestMsg;
        }

        public static VyTaskResult<T> ProcessHttpResponse<T>(HttpResponseMessage response, RequestData requestData)
        {
            var result = new VyTaskResult<T>()
            {
                Success = false
            };

            //Error Handling
            string responsePayload = string.Empty;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                try
                {
                    responsePayload = response.Content.ReadAsStringAsync().Result;
                }
                catch (Exception)
                {
                    // ignored
                }

                if (string.IsNullOrEmpty(responsePayload))
                {
                    result.Exception = VenlyException.HttpResponseError(response, requestData);
                    return result;
                }

                //Temp hack
                VyResponse apiResponse = null;
                if (responsePayload.StartsWith("["))
                {
                    apiResponse = new VyResponse()
                    {
                        Success = false,
                        Errors = JArray.Parse(responsePayload).ToObject<VyReponseError[]>()
                    };
                }
                else
                {
                    apiResponse = JsonConvert.DeserializeObject<VyResponse>(responsePayload);
                }

                result.Exception = VenlyException.ApiResponseError(apiResponse, response, requestData);
                return result;
            }

            //Response Parsing
            try
            {
                responsePayload = response.Content.ReadAsStringAsync().Result;

                if (!requestData.RequiredWrapping)
                {
                    //NO WRAPPING REQUIRED
                    if (requestData.MustSelectProperty)
                    {
                        //SELECT PROPERTY
                        var json = JObject.Parse(responsePayload);
                        if (json.ContainsKey(requestData.SelectPropertyName))
                        {
                            result.Data =
                                JsonConvert.DeserializeObject<T>(json[requestData.SelectPropertyName].ToString());
                            result.Success = true;
                        }
                        else
                        {
                            result.Exception = VenlyException.ParsingError(
                                $"Property \'{requestData.SelectPropertyName}\' was not found in the response data");
                        }
                    }
                    else
                    {
                        //NO PROPERTY SELECTION
                        result.Data = JsonConvert.DeserializeObject<T>(responsePayload);
                        result.Success = true;
                    }
                }
                else
                {
                    //WRAPPING REQUIRED
                    if (requestData.MustSelectProperty)
                    {
                        //SELECT PROPERTY
                        var apiResponse = JsonConvert.DeserializeObject<VyResponse<JObject>>(responsePayload);
                        if (apiResponse.Success)
                        {
                            if (apiResponse.Data.ContainsKey(requestData.SelectPropertyName))
                            {
                                result.Data =
                                    JsonConvert.DeserializeObject<T>(apiResponse.Data[requestData.MustSelectProperty]
                                        .ToString());
                                result.Success = true;
                            }
                            else
                            {
                                result.Exception = VenlyException.ParsingError(
                                    $"Property \'{requestData.SelectPropertyName}\' was not found in the response data");
                            }
                        }
                        else
                        {
                            result.Exception = VenlyException.ApiResponseError(apiResponse, response, requestData);
                        }
                    }
                    else
                    {
                        //NO PROPERTY SELECTION
                        var apiResponse = JsonConvert.DeserializeObject<VyResponse<T>>(responsePayload);
                        if (apiResponse.Success)
                        {
                            result.Data = apiResponse.Data;
                            result.Success = true;
                        }
                        else
                        {
                            result.Exception = VenlyException.ApiResponseError(apiResponse, response, requestData);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                result.Exception = VenlyException.ParsingError($"Failed to parse the response.\nErrorMsg = {e.Message}\n\nResponseText = {responsePayload}))");
            }

            return result;
        }
    }
}
