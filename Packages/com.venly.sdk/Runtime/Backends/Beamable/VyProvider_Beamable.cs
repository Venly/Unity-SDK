using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common;
using Newtonsoft.Json;
using UnityEngine;
using Venly.Core;
using Venly.Models.Shared;
using Venly.Utils;

//#if UNITY_EDITOR
//    using UnityEditor;
//#endif

namespace Venly.Backends.Beamable
{
    public class VyProvider_Beamable : VyProviderBase
    {
        public const string BeamContextDataKey = "Beamable-Context";
        private BeamContext _currContext = null;

        private bool HasContext => _currContext != null;

        private VyBeamMicroserviceInvoker _msInvoker;
        private bool HasInvoker => _msInvoker != null;

        public VyProvider_Beamable() : base(eVyBackendProvider.Beamable.GetMemberName())
        {
            ExtensionsSupported = true;
        }

//#if UNITY_EDITOR
//        [InitializeOnLoadMethod]
//#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RegisterProvider()
        {
            VenlyAPI.RegisterProvider(new VyProvider_Beamable());
        }

        protected override void OnSetData(string key, object data)
        {
            if (key.Equals(BeamContextDataKey))
            {
                if(!LoadInvoker(data as BeamContext, out var errMsg))
                {
                    VenlyLog.Exception(errMsg);
                }
            }
        }

        private bool LoadInvoker(BeamContext ctx, out string errMsg)
        {
            errMsg = string.Empty;

            //Retrieve Invoker
            if(!HasInvoker)
            {
                var clientName = VenlySettings.BeamableBackendSettings.MicroserviceName;
                var functionName = VenlySettings.BeamableBackendSettings.EntryFunctionName;
                _msInvoker = VyBeamUtils.FindMicroserviceClient(clientName, functionName, out errMsg);

                if(_msInvoker == null)
                {
                    //Invoker not found...
                    return false;
                }
            }

            //Activate Invoker
            if (ctx == null)
            {
                ctx = BeamContext.Default;

                //todo. fix blocking code
                async void waitReady()
                {
                    await ctx.OnReady;
                }

                new Task(waitReady).RunSynchronously();
            }

            _currContext = ctx;
            if (!_currContext.IsInitialized)
            {
                errMsg = "VyProvider_Beamable::LoadInvoker >> BeamContext not yet initialized. (abort)";
                _currContext = null;
                return false;
            }

            if(!_msInvoker.ActivateInstance(_currContext))
            {
                errMsg = "VyProvider_Beamable::LoadInvoker >> Failed to activate invoker. (abort)";
                return false;
            }

            return true;
        }

        public override VyTask<T> MakeRequest<T>(VyRequestData requestData)
        {
            if (!HasInvoker)
            {
                if(!LoadInvoker(_currContext, out var errMsg))
                {
                    VenlyLog.Exception(errMsg);
                    return VyTask<T>.Failed(new Exception($"[Beamable Provider] {errMsg}"));
                }
            }

            var taskNotifier = VyTask<T>.Create("beamable_provider_makeRequest");
            var resultPromise = _msInvoker.Invoke(requestData);

            //Wait for result
            resultPromise
                .Then(result =>
                {
                    try
                    {
                        var decodedResult = Encoding.UTF8.GetString(Convert.FromBase64String(result));
                        var serverResponse = JsonConvert.DeserializeObject<VyServerResponseDto>(decodedResult);
                        VyTaskResult<T> taskResult;

                        if (serverResponse.IsDataEncoded)
                        {
                            serverResponse.Data = Encoding.UTF8.GetString(Convert.FromBase64String(serverResponse.Data));
                        }

                        if (serverResponse is { Success: true })
                        {
                            if (serverResponse.IsRawResponse) //Requires Response Parsing
                            {
                                //Small trick to safely unescape the JSON encoded JSON string...
                                //object decodedJsonString = JsonConvert.DeserializeObject(serverResponse.Data);
                                taskResult = VenlyUtils.ProcessApiResponse<T>(serverResponse.Data, (HttpStatusCode)serverResponse.StatusCode, requestData);
                                taskNotifier.Notify(taskResult);
                                return;
                            }

                            //Response contains the requested object, no parsing required
                            T data = JsonConvert.DeserializeObject<T>(serverResponse.Data);
                            taskResult = new VyTaskResult<T>(data);
                            taskNotifier.Notify(taskResult);
                            return;
                        }

                        taskResult = new VyTaskResult<T>(new VyException(serverResponse.ErrorMessage));
                        taskNotifier.Notify(taskResult);
                    }
                    catch (Exception ex)
                    {
                        taskNotifier.NotifyFail(new VyException(ex));
                    }
                })
                .Error(ex =>
                {
                    taskNotifier.NotifyFail(ex);
                });

            return taskNotifier.Task;
        }
    }
}
