using System.Collections.Generic;
using System.Net.Http;
using Proto.Promises;
using Venly.Models;
using Venly.Models.Internal;
using Venly.Utils;

namespace Venly
{
    public static partial class VenlyAPI
    {
#if UNITY_EDITOR || UNITY_SERVER || ENABLE_VENLY_AZURE
        public static class Server
        {
            public static class Authentication
            {
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
            }

            public static class Wallet
            {
                private static readonly eVyApiEndpoint _apiEndpoint = eVyApiEndpoint.Wallet;

                /// <summary>
                /// Create a new wallet
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>New wallet</returns>
                public static VyTask<VyWallet> CreateWallet(VyParam_CreateWallet reqParams)
                {
                    var reqData = RequestData.Post("/api/wallets", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyWallet>(reqData);
                }

                /// <summary>
                /// Transfer a native token
                /// </summary>
                /// <param name="reqParams">(Required) parameters for the request</param>
                /// <returns>Transfer Transaction Info</returns>
                public static VyTask<VyTransferInfo> ExecuteTransfer(VyParam_ExecuteTransfer reqParams)
                {
                    var reqData = RequestData.Post("/api/transactions/execute", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTransferInfo>(reqData);
                }

                /// <summary>
                /// Transfer a Fungible Token (FT)
                /// </summary>
                /// <param name="reqParams">(Required) parameters for the request</param>
                /// <returns>Transfer Transaction Info</returns>
                public static VyTask<VyTransferInfo> ExecuteTokenTransfer(VyParam_ExecuteTokenTransfer reqParams)
                {
                    var reqData = RequestData.Post("/api/transactions/execute", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTransferInfo>(reqData);
                }

                /// <summary>
                /// Transfer a Non Fungible Token (NFT)
                /// </summary>
                /// <param name="reqParams">(Required) parameters for the request</param>
                /// <returns>Transfer Transaction Info</returns>
                public static VyTask<VyTransferInfo> ExecuteNftTransfer(VyParam_ExecuteNftTransfer reqParams)
                {
                    var reqData = RequestData.Post("/api/transactions/execute", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTransferInfo>(reqData);
                }

                /// <summary>
                /// Execute a function on a smart contract (write) on any (supported) BlockChain
                /// </summary>
                /// <param name="reqParams">(Required) parameters for the request</param>
                /// <returns>Execution Transaction Info</returns>
                public static VyTask<VyTransferInfo> ExecuteContract(VyParam_ExecuteContract reqParams)
                {
                    var reqData = RequestData.Post("/api/transactions/execute", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTransferInfo>(reqData);
                }

                /// <summary>
                /// Read from a smart contract function
                /// [/api/contracts/read]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Output values from the function</returns>
                public static VyTask<VyTypeValuePair[]> ReadContract(VyParam_ReadContract reqParams)
                {
                    var reqData = RequestData.Post("/api/contracts/read", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTypeValuePair[]>(reqData);
                }

                /// <summary>
                /// Resubmit an existing transaction which failed to execute/propagate
                /// [/api/transactions/resubmit]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Transaction Info</returns>
                public static VyTask<VyTransferInfo> ResubmitTransaction(VyParam_ResubmitTransaction reqParams)
                {
                    var reqData = RequestData.Post("/api/transactions/resubmit", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTransferInfo>(reqData);
                }

                /// <summary>
                /// Export a Venly Wallet
                /// [/api/wallets/:walletid/export]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Keystore of the exported wallet</returns>
                public static VyTask<VyWalletExport> ExportWallet(VyParam_ExportWallet reqParams)
                {
                    var reqData = RequestData.Post($"/api/wallets/{reqParams.WalletId}/export", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyWalletExport>(reqData);
                }

                /// <summary>
                /// Import an external wallet (Keystore, PrivateKey or WIF)
                /// [/api/wallets/import]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>The Imported Wallet</returns>
                public static VyTask<VyWallet> ImportWallet(VyParam_ImportWallet reqParams)
                {
                    var reqData = RequestData.Post("/api/wallets/import", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyWallet>(reqData);
                }

                /// <summary>
                /// Reset the PIN of a specific Wallet
                /// [/api/wallets/:walletId/security]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>The Updated Wallet</returns>
                public static VyTask<VyWallet> ResetPin(VyParam_UpdateWalletSecurity reqParams)
                {
                    var reqData = RequestData.Patch($"/api/wallets/{reqParams.WalletId}/security", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyWallet>(reqData);
                }
            }

            public static class NFT
            {
                private static readonly eVyApiEndpoint _apiEndpoint = eVyApiEndpoint.Nft;

                //todo: remove applicationID in params
                /// <summary>
                /// Deploy a new smart contract (NFT Contract) on a specific BlockChain
                /// [/api/minter/contracts]
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>The deployed NFT Contract</returns>
                public static VyTask<VyContract> CreateContract(VyParam_CreateContract reqParams)
                {
                    var reqData = RequestData.Post("/api/minter/contracts", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyContract>(reqData);
                }

                //todo remove ApplicationID
                /// <summary>
                /// Create an NFT Token-Type (Template) which you can use to mint NFTs from
                /// [/api/minter/contracts/:contractId/token-types]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>The deployed NFT Token-Type (Template)</returns>
                public static VyTask<VyTokenType> CreateTokenType(VyParam_CreateTokenType reqParams)
                {
                    var reqData = RequestData.Post($"/api/minter/contracts/{reqParams.ContractId}/token-types", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyTokenType>(reqData);
                }

                /// <summary>
                /// Mint a Non-Fungible Token (NFT) based on a specific Token-Type (Template)
                /// [/api/minter/contracts/:contractId/tokens/non-fungible]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Information on the Minted Tokens</returns>
                public static VyTask<VyMintedTokenInfo> MintTokenNFT(VyParam_MintNFT reqParams)
                {
                    var reqData = RequestData.Post($"/api/minter/contracts/{reqParams.ContractId}/tokens/non-fungible", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyMintedTokenInfo>(reqData);
                }

                /// <summary>
                /// Mint a Fungible Token (FT) based on a specific Token-Type (Template)
                /// [/api/minter/contracts/:contractId/tokens/fungible]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Information on the Minted Tokens</returns>
                public static VyTask<VyMintedTokenInfo> MintTokenFT(VyParam_MintFT reqParams)
                {
                    var reqData = RequestData.Post($"/api/minter/contracts/{reqParams.ContractId}/tokens/fungible", _apiEndpoint)
                        .AddJsonContent(reqParams);
                    return Request<VyMintedTokenInfo>(reqData);
                }

                /// <summary>
                /// Update the metadata of a Token-Type (NFT Template)
                /// [/api/contracts/:contractId/token-types/:tokenTypeId/metadata]
                /// </summary>
                /// <param name="reqParams">Required parameters for the request</param>
                /// <returns>Updated Token-Type (Template)</returns>
                public static VyTask<VyTokenTypeMetadata> UpdateTokenTypeMetadata(VyParam_UpdateTokenTypeMetadata reqParams)
                {
                    var reqData = RequestData.Put($"/api/contracts/{reqParams.ContractId}/token-types/{reqParams.TokenTypeId}/metadata", _apiEndpoint)
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
                    var reqData = RequestData.Patch($"/api/contracts/{reqParams.ContractId}/metadata", _apiEndpoint)
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
                    var reqData = RequestData.Delete($"/api/minter/contracts/{contractId}", _apiEndpoint);
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
                    var reqData = RequestData.Delete($"/api/minter/contracts/{contractId}/token-types/{tokenTypeId}", _apiEndpoint);
                    return Request<VyTaskVoid>(reqData);
                }
            }

            //public static class Market
            //{

            //}
        }
#endif
    }
}