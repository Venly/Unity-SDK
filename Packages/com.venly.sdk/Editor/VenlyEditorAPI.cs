using System;
using System.Collections.Generic;
using Packages.com.venly.sdk.Editor;
using Venly.Core;
using Venly.Models.Token;
using Venly.Models.Shared;
using Venly.Editor.Utils;

namespace Venly.Editor
{
#if UNITY_EDITOR
    internal static class VenlyEditorAPI
    {
        public static bool IsInitialized = false;
        private static VyProvider_Editor _provider;

        static VenlyEditorAPI()
        {
            _provider = new VyProvider_Editor();
            IsInitialized = true;
        }

        #region AUTH
        public static VyTask<VyAccessTokenDto> GetAccessToken(string clientId, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return VyTask<VyAccessTokenDto>.Failed(VyException.Argument("Client-ID cannot be NULL or empty", nameof(clientId)));

            if (string.IsNullOrWhiteSpace(clientSecret))
                return VyTask<VyAccessTokenDto>.Failed(VyException.Argument("Client-SECRET cannot be NULL or empty", nameof(clientSecret)));

            var formData = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"client_id", clientId},
                {"client_secret", clientSecret}
            };
            var reqData = VyRequestData.Post("/auth/realms/Arkane/protocol/openid-connect/token", eVyApiEndpoint.Auth)
                .AddFormContent(formData);

            return Request<VyAccessTokenDto>(reqData);
        }
        #endregion

        #region SERVER
        /// <summary>
        /// Deploy a new smart contract (NFT Contract) on a specific BlockChain
        /// [/api/minter/contracts]
        /// <param name="reqParams">Required parameters for the request</param>
        /// <returns>The deployed NFT Contract</returns>
        public static VyTask<VyDeployedErc1155ContractDto> CreateContract(VyCreateErc1155ContractRequest reqParams)
        {
            //Todo: check url
            var reqData = VyRequestData.Post("/api/minter/contracts", eVyApiEndpoint.Token)
                .AddJsonContent(reqParams);
            return Request<VyDeployedErc1155ContractDto>(reqData);
        }

        /// <summary>
        /// Create an NFT Token-Type (Template) which you can use to mint NFTs from
        /// [/api/minter/contracts/:contractId/token-types]
        /// </summary>
        /// <param name="reqParams">Required parameters for the request</param>
        /// <returns>The deployed NFT Token-Type (Template)</returns>
        public static VyTask<VyErc1155TokenTypeDto> CreateTokenType(int contractId, VyCreateErc1155TokenTypeRequest reqParams)
        {
            //Todo: check url
            var reqData = VyRequestData.Post($"/api/minter/contracts/{contractId}/token-types", eVyApiEndpoint.Token)
                .AddJsonContent(reqParams);
            return Request<VyErc1155TokenTypeDto>(reqData);
        }

        /// <summary>
        /// Update the metadata of a Token-Type (NFT Template)
        /// [/api/contracts/:contractId/token-types/:tokenTypeId/metadata]
        /// </summary>
        /// <param name="reqParams">Required parameters for the request</param>
        /// <returns>Updated Token-Type (Template)</returns>
        public static VyTask<VyMetadataDto> UpdateTokenTypeMetadata(int contractId, int tokenTypeId, VyUpdateErc1155TokenTypeMetadataRequest reqParams)
        {
            //Todo: check url
            var reqData = VyRequestData.Put($"/api/contracts/{contractId}/token-types/{tokenTypeId}/metadata", eVyApiEndpoint.Token)
                .AddJsonContent(reqParams);
            return Request<VyMetadataDto>(reqData);
        }

        //todo: check with backend team about this response
        /// <summary>
        /// Update the metadata of a Contract
        /// [/api/contracts/:contractId/metadata]
        /// </summary>
        /// <param name="reqParams">Required parameters for the request</param>
        /// <returns>Updated Contract Metadata</returns>
        public static VyTask<VyErc1155TokenContractMetadataDto> UpdateContractMetadata(int contractId, VyUpdateErc1155ContractMetadataRequest reqParams)
        {
            var reqData = VyRequestData.Patch($"/api/contracts/{contractId}/metadata", eVyApiEndpoint.Token)
                .AddJsonContent(reqParams);
            return Request<VyErc1155TokenContractMetadataDto>(reqData);
        }

        /// <summary>
        /// Archive a specific Contract. When a contract is archived it is removed from your account together with all the token-types that are associated to that contract
        /// [/api/minter/contracts/:contractId]
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns> void promise </returns>
        public static VyTask<VyTaskVoid> ArchiveContract(int contractId)
        {
            //Todo: check url
            var reqData = VyRequestData.Delete($"/api/minter/contracts/{contractId}", eVyApiEndpoint.Token);
            return Request<VyTaskVoid>(reqData);
        }

        /// <summary>
        /// Archive a specific Token Type (Template)
        /// [/api/minter/contracts/:contractId/token-types/:tokenTypeId]
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="tokenTypeId"></param>
        /// <returns> void promise </returns>
        public static VyTask<VyTaskVoid> ArchiveTokenType(int contractId, int tokenTypeId)
        {
            //Todo: check url
            var reqData = VyRequestData.Delete($"/api/minter/contracts/{contractId}/token-types/{tokenTypeId}", eVyApiEndpoint.Token);
            return Request<VyTaskVoid>(reqData);
        }
        #endregion

        #region CLIENT
        /// <summary>
        /// Retrieve the supported chains for the Wallet API
        /// [/api/chains]
        /// </summary>
        /// <returns>List of supported chains</returns>
        public static VyTask<eVyChain[]> GetChainsWALLET()
        {
            //var taskNotifier = VyTask<eVyChain[]>.Create();

            var reqData = VyRequestData.Get("/api/chains", eVyApiEndpoint.Wallet);
            return Request<eVyChain[]>(reqData);
            //Request<eVyChain[]>(reqData)
            //    .OnComplete(result =>
            //    {
            //        if (result.Success)
            //        {
            //            taskNotifier.NotifySuccess(VenlyEditorUtils.TrimUnsupportedChains(result.Data));
            //        }
            //        else
            //        {
            //            taskNotifier.NotifyFail(result.Exception);
            //        }
            //    });

            //return taskNotifier.Task;
        }

        /// <summary>
        /// Retrieve a list of all the BlockChains that are supported by the NFT API
        /// [/api/env]
        /// </summary>
        /// <returns>List of supported BlockChains</returns>
        public static VyTask<eVyChain[]> GetChainsNFT()
        {
            //var taskNotifier = VyTask<eVyChain[]>.Create();

            var reqData = VyRequestData.Get("/api/env", eVyApiEndpoint.Token)
                .SelectProperty("supportedChainsForItemCreation");

            return Request<eVyChain[]>(reqData);

            //Request<eVyChain[]>(reqData)
            //    .OnComplete(result =>
            //    {
            //        if (result.Success)
            //        {
            //            taskNotifier.NotifySuccess(VenlyEditorUtils.TrimUnsupportedChains(result.Data));
            //        }
            //        else
            //        {
            //            taskNotifier.NotifyFail(result.Exception);
            //        }
            //    });

            //return taskNotifier.Task;
        }

        /// <summary>
        /// Retrieve information of all the NFT contracts associated with a specific application ID
        /// [/api/minter/contracts]
        /// </summary>
        /// <returns>List of Contract Information</returns>
        public static VyTask<VyErc1155ContractDto[]> GetContracts()
        {
            //Todo: check url
            //VenlyAPI.Token.GetErc1155Contracts()
            var reqData = VyRequestData.Get("/api/minter/contracts", eVyApiEndpoint.Token);
            return Request<VyErc1155ContractDto[]>(reqData);
        }

        /// <summary>
        /// Retrieve information of a single NFT contract associated with a specific application ID
        /// [/api/minter/contracts/:contractId]
        /// </summary>
        /// <param name="contractId">The ID of the contract you want the information from</param>
        /// <returns>Contract Information</returns>
        public static VyTask<VyErc1155ContractDto> GetContract(int contractId)
        {
            //Todo: check url
            var reqData = VyRequestData.Get($"/api/minter/contracts/{contractId}", eVyApiEndpoint.Token);
            return Request<VyErc1155ContractDto>(reqData);
        }

        /// <summary>
        /// Retrieve information of all NFT token types (templates) from one of your contracts
        /// [/api/minter/contracts/:contractId/token-types]
        /// </summary>
        /// <param name="contractId">The ID of the contract you want the token type information from</param>
        /// <returns>List NFT token type (template) information</returns>
        public static VyTask<VyErc1155TokenTypeDto[]> GetTokenTypes(int contractId)
        {
            //Todo: check url
            var reqData = VyRequestData.Get($"/api/minter/contracts/{contractId}/token-types", eVyApiEndpoint.Token);
            return Request<VyErc1155TokenTypeDto[]>(reqData);
        }

        /// <summary>
        /// Retrieve information of a single token type (template) from one of your contracts
        /// [/api/minter/contracts/:contractId/token-types/:tokenTypeId]
        /// </summary>
        /// <param name="contractId">The ID of the contract</param>
        /// <param name="tokenTypeId">The ID of the token type (template)</param>
        /// <returns>NFT token type (template) Information</returns>
        public static VyTask<VyErc1155TokenTypeDto> GetTokenType(int contractId, int tokenTypeId)
        {
            //Todo: check url
            var reqData = VyRequestData.Get($"/api/minter/contracts/{contractId}/token-types/{tokenTypeId}", eVyApiEndpoint.Token);
            return Request<VyErc1155TokenTypeDto>(reqData);
        }
        #endregion


        #region Request Helpers
        private static Exception VerifyRequest()
        {
            if (!VyEditorData.IsLoaded)
            {
                VyEditorData.Reload();
                VenlyDebugEd.LogDebug("[VenlyEditorAPI] VenlySettings Force Loading", 1);
            }

            if (!IsInitialized) return new VyException("VenlyEditorAPI not yet initialized!");
            if (_provider == null) return new VyException("VenlyAPI Editor Provider is null");

            return null!;
        }

        private static VyTask<T> Request<T>(VyRequestData requestData)
        {
            var ex = VerifyRequest();
            return ex != null ? VyTask<T>.Failed(ex) : _provider.MakeRequest<T>(requestData);
        }
        #endregion
    }
#endif
}
