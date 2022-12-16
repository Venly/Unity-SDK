using Newtonsoft.Json;
using Proto.Promises;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Venly.Models;
using Venly.Models.Internal;
using Venly.Utils;

namespace Venly.Backends.Local
{
    public class LocalRequester : IVenlyRequester
    {
        private VyAccessToken _accessToken;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public LocalRequester(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        private VyTask<bool> Authenticate()
        {
#if UNITY_EDITOR
            var taskNotifier = VyTask<bool>.Create("Authenticate");
            //var deferred = VyDeferredTask<bool>.Create();

            VenlyAPI.Server.Authentication.GetAccessToken(_clientId, _clientSecret)
                .OnSucces(token =>
                {
                    _accessToken = token;
                    taskNotifier.NotifySuccess(true);
                })
                .OnFail(ex => taskNotifier.NotifyFail(ex));
           
            return taskNotifier.Task;
#else
            return  VyTask.Failed(new Exception("[Venly SDK] Local Requester can only be used inside the Unity Editor."), "Authenticate Forbidden");
#endif
        }

        private VyTask<bool> ValidateAccessToken(bool skip = false)
        {
            if (skip || _accessToken is {IsValid: true}) return VyTask<bool>.Succeeded(true, "ValidateAccessToken");

            return Authenticate();
        }

        //Make Request
        //protected override Promise<T> MakeRequest<T>(HttpMethod method, string uri, eVyApiEndpoint endpoint,
        //    HttpContent content, bool isWrapped)
        //{
        //    return Promise<T>.Rejected(new NotImplementedException());
        //    //var result = Promise<T>.NewDeferred();

        //    ////Check if we need to start another Thread
        //    //if (Thread.CurrentThread.IsBackground) HttpRequestAction();
        //    //else Task.Run(HttpRequestAction);

        //    ////The Action (todo: put in utils)
        //    //void HttpRequestAction()
        //    //{
        //    //    //Check if Authorization is required
        //    //    bool requiresAuthorization = endpoint != eVyApiEndpoint.Auth;

        //    //    //Validate Token if required
        //    //    ValidateAccessToken(!requiresAuthorization)
        //    //        .Then(async () =>
        //    //        {
        //    //            using HttpClient client = new();

        //    //            //Build Request
        //    //            //-------------
        //    //            var request = new HttpRequestMessage(method, VenlyUtils.GetUrl(uri, endpoint, VenlySettings.Environment))
        //    //                {Content = content};

        //    //            request.Headers.Add("Accept", "application/json");


        //    //            if (requiresAuthorization)
        //    //            {
        //    //                request.Headers.Add("Authorization", $"Bearer {_accessToken.Token}");
        //    //            }

        //    //            //Send Request
        //    //            //============
        //    //            var response = await client.SendAsync(request);

        //    //            //todo: check internal server error handling (if error message
        //    //            //if (response.StatusCode < HttpStatusCode.InternalServerError)
        //    //            {
        //    //                var responsePayload = await response.Content.ReadAsStringAsync();
        //    //                T payload = default;
        //    //                if (typeof(T) == typeof(string)) //todo: remove string specialization (check first)
        //    //                {
        //    //                    payload = (T) (responsePayload as object);
        //    //                    result.Resolve(payload);
        //    //                }
        //    //                else
        //    //                {
        //    //                    try
        //    //                    {
        //    //                        payload = JsonConvert.DeserializeObject<T>(responsePayload);
        //    //                        result.Resolve(payload);
        //    //                    }
        //    //                    catch (Exception e)
        //    //                    {
        //    //                        throw VenlyUtils.WrapException(
        //    //                            $"Failed to parse the response.\nErrorMsg = {e.Message}\n\nResponseText = {responsePayload}))");
        //    //                    }
        //    //                }
        //    //            }
        //    //            //else
        //    //            //{
        //    //            //    var errorMessage = await response.Content.ReadAsStringAsync();
        //    //            //    result.Reject(new Exception($"[Local-Requester] Failed to sent the request. ({response.ReasonPhrase})"));
        //    //            //}
        //    //        })
        //    //        .CatchAndForget<T>(result);
        //    //}

        //    //return result.Promise;
        //}

        public override VyTask<T> MakeRequest<T>(RequestData requestData)
        {
            //Create Task
            var taskNotifier = VyTask<T>.Create("MakeRequest");

            //Check if we need to start another Thread
            if (Thread.CurrentThread.IsBackground) HttpRequestAction();
            else Task.Run(HttpRequestAction);

            async void HttpRequestAction()
            {
                try
                {
                    //Check if Authorization is required
                    bool requiresAuthorization = requestData.Endpoint != eVyApiEndpoint.Auth;

                    //Validate Token
                    //--------------
                    await ValidateAccessToken(!requiresAuthorization);

                    //Execute Request
                    //-------------
                    var requestMessage = requestData.ToRequestMessage(requiresAuthorization ? _accessToken : null);

                    using HttpClient client = new();
                    var response = await client.SendAsync(requestMessage);
                    var result = VenlyUtils.ProcessHttpResponse<T>(response, requestData);

                    //Little Hack
                    //var resultJson = JsonConvert.SerializeObject(result);
                    taskNotifier.Notify(result);
                }
                catch (Exception ex)
                {
                    taskNotifier.NotifyFail(ex);
                }
            }

            return taskNotifier.Task;
        }
    }
}