using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Proto.Promises;
using Venly.Models;
using Venly.Models.Internal;

namespace Venly.Editor
{
#if UNITY_EDITOR
    internal static class VenlyEditorAPI
    {
        public static bool IsInitialized = false;
        private static VenlyEditorRequester _requester;

        private static bool _useWrapNFT = false;

        static VenlyEditorAPI()
        {
            _requester = new VenlyEditorRequester();
            IsInitialized = true;

            Promise.Config.ForegroundContext = SynchronizationContext.Current;
        }

        #region AUTH
        public static VyTask<VyAccessToken> GetAccessToken(string clientId, string clientSecret)
        {
            var formData = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"client_id", clientId},
                {"client_secret", clientSecret}
            };

            var reqData = RequestData.Post("/auth/realms/Arkane/protocol/openid-connect/token", eVyApiEndpoint.Auth)
                .AddFormContent(formData);
            return Request<VyAccessToken>(reqData);
        }
        #endregion

        #region SERVER
        /// <summary>
        /// Deploy a new smart contract (NFT Contract) on a specific BlockChain
        /// [/api/minter/contracts]
        /// <param name="reqParams">Required parameters for the request</param>
        /// <returns>The deployed NFT Contract</returns>
        public static VyTask<VyContract> CreateContract(VyParam_CreateContract reqParams)
        {
            var reqData = RequestData.Post("/api/minter/contracts", eVyApiEndpoint.Nft)
                .AddJsonContent(reqParams);
            return Request<VyContract>(reqData);
        }

        /// <summary>
        /// Create an NFT Token-Type (Template) which you can use to mint NFTs from
        /// [/api/minter/contracts/:contractId/token-types]
        /// </summary>
        /// <param name="reqParams">Required parameters for the request</param>
        /// <returns>The deployed NFT Token-Type (Template)</returns>
        public static VyTask<VyTokenType> CreateTokenType(VyParam_CreateTokenType reqParams)
        {
            var reqData = RequestData.Post($"/api/minter/contracts/{reqParams.ContractId}/token-types", eVyApiEndpoint.Nft)
                .AddJsonContent(reqParams);
            return Request<VyTokenType>(reqData);
        }

        /// <summary>
        /// Update the metadata of a Token-Type (NFT Template)
        /// [/api/contracts/:contractId/token-types/:tokenTypeId/metadata]
        /// </summary>
        /// <param name="reqParams">Required parameters for the request</param>
        /// <returns>Updated Token-Type (Template)</returns>
        public static VyTask<VyTokenTypeMetadata> UpdateTokenTypeMetadata(VyParam_UpdateTokenTypeMetadata reqParams)
        {
            var reqData = RequestData.Put($"/api/contracts/{reqParams.ContractId}/token-types/{reqParams.TokenTypeId}/metadata", eVyApiEndpoint.Nft)
                .AddJsonContent(reqParams);
            return Request<VyTokenTypeMetadata>(reqData);
        }

        //todo: check with backend team about this response
        /// <summary>
        /// Update the metadata of a Contract
        /// [/api/contracts/:contractId/metadata]
        /// </summary>
        /// <param name="reqParams">Required parameters for the request</param>
        /// <returns>Updated Contract Metadata</returns>
        public static VyTask<VyContractMetadata> UpdateContractMetadata(VyParam_UpdateContractMetadata reqParams)
        {
            var reqData = RequestData.Patch($"/api/contracts/{reqParams.ContractId}/metadata", eVyApiEndpoint.Nft)
                .AddJsonContent(reqParams);
            return Request<VyContractMetadata>(reqData);
        }

        /// <summary>
        /// Archive a specific Contract. When a contract is archived it is removed from your account together with all the token-types that are associated to that contract
        /// [/api/minter/contracts/:contractId]
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns> void promise </returns>
        public static VyTask<VyTaskVoid> ArchiveContract(string applicationId, int contractId)
        {
            var reqData = RequestData.Delete($"/api/minter/contracts/{contractId}", eVyApiEndpoint.Nft);
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
            var reqData = RequestData.Delete($"/api/minter/contracts/{contractId}/token-types/{tokenTypeId}", eVyApiEndpoint.Nft);
            return Request<VyTaskVoid>(reqData);
        }
        #endregion

        #region CLIENT
        public static VyTask<VyApp[]> GetApps()
        {
            var reqData = RequestData.Get("/api/apps", eVyApiEndpoint.Nft);
            return Request<VyApp[]>(reqData);
        }

        /// <summary>
        /// Retrieve information of all the NFT contracts associated with a specific application ID
        /// [/api/minter/contracts]
        /// </summary>
        /// <returns>List of Contract Information</returns>
        public static VyTask<VyContract[]> GetContracts()
        {
            var reqData = RequestData.Get("/api/minter/contracts", eVyApiEndpoint.Nft);
            return Request<VyContract[]>(reqData);
        }

        /// <summary>
        /// Retrieve information of a single NFT contract associated with a specific application ID
        /// [/api/minter/contracts/:contractId]
        /// </summary>
        /// <param name="contractId">The ID of the contract you want the information from</param>
        /// <returns>Contract Information</returns>
        public static VyTask<VyContract> GetContract(int contractId)
        {
            var reqData = RequestData.Get($"/api/minter/contracts/{contractId}", eVyApiEndpoint.Nft);
            return Request<VyContract>(reqData);
        }

        /// <summary>
        /// Retrieve information of all NFT token types (templates) from one of your contracts
        /// [/api/minter/contracts/:contractId/token-types]
        /// </summary>
        /// <param name="contractId">The ID of the contract you want the token type information from</param>
        /// <returns>List NFT token type (template) information</returns>
        public static VyTask<VyTokenType[]> GetTokenTypes(int contractId)
        {
            var reqData = RequestData.Get($"/api/minter/contracts/{contractId}/token-types", eVyApiEndpoint.Nft);
            return Request<VyTokenType[]>(reqData);
        }

        /// <summary>
        /// Retrieve information of a single token type (template) from one of your contracts
        /// [/api/minter/contracts/:contractId/token-types/:tokenTypeId]
        /// </summary>
        /// <param name="contractId">The ID of the contract</param>
        /// <param name="tokenTypeId">The ID of the token type (template)</param>
        /// <returns>NFT token type (template) Information</returns>
        public static VyTask<VyTokenType> GetTokenType(int contractId, int tokenTypeId)
        {
            var reqData = RequestData.Get($"/api/minter/contracts/{contractId}/token-types/{tokenTypeId}", eVyApiEndpoint.Nft);
            return Request<VyTokenType>(reqData);
        }
        #endregion


        #region Request Helpers
        private static Exception VerifyRequest()
        {
            if (!IsInitialized) return new VenlyException("VenlyEditorAPI not yet initialized!");
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
#endif
}
