using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Proto.Promises;
using UnityEditor.PackageManager;
using Venly.Backends;
using Venly.Models;
using Venly.Models.Internal;

#if ENABLE_VENLY_AZURE
using Venly.Azure;
#endif

namespace Venly
{
    public static partial class VenlyAPI
    {
        public static bool IsInitialized { get; private set; }
        public static eVyEnvironment CurrentEnvironement { get; private set; }

        public static IBackendExtension BackendExtension => _currentProvider?.Extensions;
        private static IVenlyRequester _requester => _currentProvider?.Requester;

        private static readonly List<BackendProvider> _backendProviders = new();
        private static BackendProvider _currentProvider;



#if ENABLE_VENLY_AZURE
        public static IVenlyRequester Requester => _requester;

        public static void SetEnvironment(eVyEnvironment env)
        {
            CurrentEnvironement = env;
        }

        static VenlyAPI()
        {
            var azureProvider = new AzureProvider();
            azureProvider.Initialize();

            var envStr = AzureUtils.GetEnvVar(AzureUtils.VENLY_ENV_KEY);
            var envType = envStr.Equals("staging", StringComparison.OrdinalIgnoreCase)
                ? eVyEnvironment.staging
                : eVyEnvironment.production;

            Initialize(azureProvider, envType);
        }
#else
        //Register a Provider that can be used during API Initialization
        public static void RegisterProvider(BackendProvider provider)
        {
            //Check if singleInstance type provider is already registered...
            if (provider.ProviderType != eVyBackendProvider.Custom)
            {
                if (_backendProviders.Any(p => p.ProviderType == provider.ProviderType))
                {
                    throw new VenlyException($"A Backend Provider with type \'{provider.ProviderType}\' is already registered. (Multi-registration is only allowed for Custom Providers)");
                }
            }

            //Add Provider
            _backendProviders.Add(provider);
        }

        public static void Initialize()
        {
            var customId = -1;
            if (VenlySettings.BackendProvider == eVyBackendProvider.Custom)
                customId = VenlySettings.CustomBackendSettings.CustomId;

            Initialize(VenlySettings.BackendProvider, customId);
        }

        //Provider Specific Initialization (Only if registered)
        public static void Initialize(eVyBackendProvider type, int customId = -1)
        {
            //Deinitialize if required
            Deinitialize();

            //Find the requester provider type
            var providerMatches = _backendProviders.FindAll(p => p.ProviderType == type);
            var provider = providerMatches.Count == 1 ? providerMatches[0] : providerMatches.FirstOrDefault(p => p.CustomId == customId);
            
            //Provider Found
            if (provider != null)
            {
                //Provider INIT
                provider.Initialize(); //todo error handling during provider init (promise? ret val?)

                //API INIT
                Initialize(provider, VenlySettings.Environment);

                return;
            }

            //Provider NOT Found
            throw new VenlyException($"Provider with type \'{type}\' is not registered.\nVenlyAPI Initialization Failed!");
        }

        public static void Initialize(BackendProvider provider)
        {
            Initialize(provider, VenlySettings.Environment);
        }
#endif

        //Custom Initialization
        internal static void Initialize(BackendProvider provider, eVyEnvironment env)
        {
            //Deinitialize if required
            Deinitialize();

            //Check if provider is Initialized
            if (!provider.IsInitialized)
            {
                throw new VenlyException($"Provided Backend Provider (\'{provider.ProviderType}\') is not yet initialized.\nVenlyAPI Intialization Failed!");
            }

            //Set Current Provider
            _currentProvider = provider;

            //Set Environment
            CurrentEnvironement = env;

            //Set Promise Debug Level
            Promise.Config.DebugCausalityTracer = Promise.TraceLevel.All;
            Promise.Config.ForegroundContext = SynchronizationContext.Current;

            //Set IsInitialized flag!
            IsInitialized = true;
        }

        private static void Deinitialize()
        {
            if (!IsInitialized) return;

            //Uninitialize the backend provider
            if (_currentProvider != null)
            {
                _currentProvider.Deinitialize();
                _currentProvider = null;
            }

            //API Uninitialize
            //...

            IsInitialized = false;
        }

        public static bool HandleProviderError(object err)
        {
            return _currentProvider != null && _currentProvider.HandleError(err);
        }

        public static void SetRequesterData(string key, object data)
        {
            if (!IsInitialized)
            {
                throw new Exception("VenlyAPI not yet initialized!");
            }

            _requester.SetData(key, data);
        }

        public static void OverrideLocalBackendExtensions(IBackendExtension extensions)
        {
            var localProvider = _backendProviders.FirstOrDefault(p => p.ProviderType == eVyBackendProvider.Local);
            if (localProvider != null)
            {
                localProvider.OverrideExtension(extensions);
            }
        }

#region Request Helpers
        private static Exception VerifyRequest()
        {
            if (!IsInitialized) return new VenlyException("VenlyAPI not yet initialized!");
            if (_requester == null) return new VenlyException("VenlyAPI requester is null");

            return null!;
        }

        private static VyTask<T> Request<T>(RequestData requestData)
        {
            var ex = VerifyRequest();
            return ex != null ? VyTask<T>.Failed(ex) : _requester.MakeRequest<T>(requestData);
        }
        #endregion
    }
}