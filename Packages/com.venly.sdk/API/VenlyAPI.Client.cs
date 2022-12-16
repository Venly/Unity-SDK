using Proto.Promises;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Venly.Models;
using Venly.Models.Internal;
using Venly.Utils;

namespace Venly
{
    public static partial class VenlyAPI
    {
        public static class Client
        {
            public static class Wallet
            {
                private static readonly eVyApiEndpoint _apiEndpoint = eVyApiEndpoint.Wallet;

                /// <summary>
                /// Retrieve the supported chains for the Wallet API
                /// [/api/chains]
                /// </summary>
                /// <returns>List of supported chains</returns>
                public static VyTask<eVySecretType[]> GetChains()
                {
                    var reqData = RequestData.Get("/api/chains", _apiEndpoint);
                    return Request<eVySecretType[]>(reqData);
                }

                /// <summary>
                /// Retrieve information on a specific BlockChain
                /// [/api/chains/:secretType]
                /// </summary>
                /// <param name="secretType">The BlockChain to get information from</param>
                /// <returns>BlockChain information</returns>
                public static VyTask<VyBlockchainInfo> GetBlockNumber(eVySecretType secretType)
                {
                    var reqData = RequestData.Get($"/api/chains/{secretType.GetMemberName()}", _apiEndpoint);
                    return Request<VyBlockchainInfo>(reqData);
                }

                /// <summary>
                /// Retrieve all the wallets associated with this client account
                /// [/api/wallets]
                /// </summary>
                /// <returns>List of Venly Wallets</returns>
                public static VyTask<VyWallet[]> GetWallets()
                {
                    var reqData = RequestData.Get("/api/wallets", _apiEndpoint);
                    return Request<VyWallet[]>(reqData);
                }

                /// <summary>
                /// Retrieve a wallet based on a wallet ID
                /// [/api/wallets/:walletId]
                /// </summary>
                /// <param name="walletId">The ID of the wallet</param>
                /// <returns>The wallet associated with the wallet ID</returns>
                public static VyTask<VyWallet> GetWallet(string walletId)
                {
                    if(string.IsNullOrEmpty(walletId))
                        return VyTask<VyWallet>.Failed(VenlyException.Argument("Parameter cannot be NULL or empty", nameof(walletId)));

                    var reqData = RequestData.Get($"/api/wallets/{walletId}", _apiEndpoint);
                    return Request<VyWallet>(reqData);
                }

                /// <summary>
                /// Retrieves all the Fungible Tokens from a specific wallet
                /// [/api/wallets/:walletId/balance/tokens]
                /// </summary>
                /// <param name="walletId">The ID of the wallet</param>
                /// <returns>List of Fungible Tokens</returns>
                public static VyTask<VyFungibleToken[]> GetTokenBalances(string walletId)
                {
                    if (string.IsNullOrEmpty(walletId))
                        return VyTask<VyFungibleToken[]>.Failed(VenlyException.Argument("Parameter cannot be NULL or empty", nameof(walletId)));

                    var reqData = RequestData.Get($"/api/wallets/{walletId}/balance/tokens", _apiEndpoint);
                    return Request<VyFungibleToken[]>(reqData);
                }

                /// <summary>
                /// Retrieves all the Non-Fungible Tokens from a specific wallet
                /// [/api/wallets/:walletId/nonfungibles]
                /// </summary>
                /// <param name="walletId">The ID of the wallet</param>
                /// <returns>List of Non-Fungible Tokens</returns>
                public static VyTask<VyNonFungibleToken[]> GetNftTokenBalances(string walletId)
                {
                    if (string.IsNullOrEmpty(walletId))
                        return VyTask<VyNonFungibleToken[]>.Failed(VenlyException.Argument("Parameter cannot be NULL or empty", nameof(walletId)));

                    var reqData = RequestData.Get($"/api/wallets/{walletId}/nonfungibles", _apiEndpoint);
                    return Request<VyNonFungibleToken[]>(reqData);
                }

                /// <summary>
                /// Retrieves all the Non-Fungible Tokens from a specific wallet for a specific BlockChain
                /// [/api/wallets/:secretType/:walletAddress/nonfungibles]
                /// </summary>
                /// <param name="walletAddress">The address of the wallet</param>
                /// <param name="secretType">The associated BlockChain</param>
                /// <returns>List of Non-Fungible Tokens</returns>
                public static VyTask<VyNonFungibleToken[]> GetNftTokenBalances(eVySecretType secretType, string walletAddress)
                {
                    if (string.IsNullOrEmpty(walletAddress))
                        return VyTask<VyNonFungibleToken[]>.Failed(VenlyException.Argument("Parameter cannot be NULL or empty", nameof(walletAddress)));

                    var reqData = RequestData.Get($"/api/wallets/{secretType.GetMemberName()}/{walletAddress}/nonfungibles", _apiEndpoint);
                    return Request<VyNonFungibleToken[]>(reqData);
                }

                /// <summary>
                /// Retrieve information on a Transaction of a specific BlockChain
                /// [/api/transactions/:secretType/:txHash/status]
                /// </summary>
                /// <param name="secretType">The associated BlockChain</param>
                /// <param name="txHash">Hash of the transaction</param>
                /// <returns>Information about the requested Transaction</returns>
                public static VyTask<VyTransactionInfo> GetTransactionInfo(eVySecretType secretType, string txHash)
                {
                    if (string.IsNullOrEmpty(txHash))
                        return VyTask<VyTransactionInfo>.Failed(VenlyException.Argument("Parameter cannot be NULL or empty", nameof(txHash)));

                    var reqData = RequestData.Get($"/api/transactions/{secretType.GetMemberName()}/{txHash}/status", _apiEndpoint);
                    return Request<VyTransactionInfo>(reqData);
                }

                /// <summary>
                /// Retrieve the wallet events of a specific wallet
                /// [/api/wallets/:walletId/events]
                /// </summary>
                /// <param name="walletId">The ID of the wallet</param>
                /// <returns>List of associated wallet events</returns>
                public static VyTask<VyWalletEvent[]> GetWalletEvents(string walletId)
                {
                    if (string.IsNullOrEmpty(walletId))
                        return VyTask<VyWalletEvent[]>.Failed(VenlyException.Argument("Parameter cannot be NULL or empty", nameof(walletId)));

                    var reqData = RequestData.Get($"/api/wallets/{walletId}/events", _apiEndpoint);
                    return Request<VyWalletEvent[]>(reqData);
                }

                /// <summary>
                /// Validate a wallet address on a specific BlockChain
                /// [/api/wallets/address/validation/:secretType/:walletAddress]
                /// </summary>
                /// <param name="secretType">The BlockChain</param>
                /// <param name="walletAddress">The Address of the wallet</param>
                /// <returns>A wallet validation</returns>
                public static VyTask<VyWalletValidation> ValidateWalletAddress(eVySecretType secretType, string walletAddress)
                {
                    if (string.IsNullOrEmpty(walletAddress))
                        return VyTask<VyWalletValidation>.Failed(VenlyException.Argument("Parameter cannot be NULL or empty", nameof(walletAddress)));

                    var reqData = RequestData.Get($"/api/wallets/address/validation/{secretType.GetMemberName()}/{walletAddress}", _apiEndpoint);
                    return Request<VyWalletValidation>(reqData);
                }

                #region SWAP
                public static class Swap
                {
                    /// <summary>
                    /// Retrieve the possible swap pairs for a specific wallet
                    /// [/api/wallets/:walletId/swaps/pairs]
                    /// </summary>
                    /// <param name="walletId">The ID of the wallet</param>
                    /// <returns>List of possible swap pairs</returns>
                    public static VyTask<VyTradingPair[]> GetPairs(string walletId)
                    {
                        if (string.IsNullOrEmpty(walletId))
                            return VyTask<VyTradingPair[]>.Failed(VenlyException.Argument("Parameter cannot be NULL or empty", nameof(walletId)));

                        var reqData = RequestData.Get($"/api/wallets/{walletId}/swaps/pairs", _apiEndpoint);
                        return Request<VyTradingPair[]>(reqData);
                    }

                    /// <summary>
                    /// Retrieve the exchange rate for a specific swap
                    /// [/api/swaps/rates?...]
                    /// </summary>
                    /// <param name="reqParams">Required parameters for this request</param>
                    /// <returns>Results of all possible exchange rates, and the best exchange rate</returns>
                    public static VyTask<VyExchangeRateResult> GetRate(VyParam_GetSwapRate reqParams)
                    {
                        var reqData = RequestData.Get("/api/swaps/rates", _apiEndpoint)
                            .AddFormContent(reqParams);
                        return Request<VyExchangeRateResult>(reqData);
                    }
                }
                #endregion
            }

            public static class NFT
            {
                private static readonly eVyApiEndpoint _apiEndpoint = eVyApiEndpoint.Nft;

                /// <summary>
                /// Retrieve a list of all the BlockChains that are supported by the NFT API
                /// [/api/env]
                /// </summary>
                /// <returns>List of supported BlockChains</returns>
                public static VyTask<eVySecretType[]> GetChains()
                {
                    var reqData = RequestData.Get("/api/env", _apiEndpoint)
                        .SelectProperty("supportedChainsForItemCreation");
                    return Request<eVySecretType[]>(reqData);
                }

                /// <summary>
                /// Retrieve information of all the NFT contracts associated with a specific application ID
                /// [/api/minter/contracts]
                /// </summary>
                /// <returns>List of Contract Information</returns>
                public static VyTask<VyContract[]> GetContracts()
                {
                    var reqData = RequestData.Get("/api/minter/contracts", _apiEndpoint);
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
                    var reqData = RequestData.Get($"/api/minter/contracts/{contractId}", _apiEndpoint);
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
                    var reqData = RequestData.Get($"/api/minter/contracts/{contractId}/token-types", _apiEndpoint);
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
                    var reqData = RequestData.Get($"/api/minter/contracts/{contractId}/token-types/{tokenTypeId}", _apiEndpoint);
                    return Request<VyTokenType>(reqData);
                }

                //todo: check model types/names/differences between template_meta vs token_meta
                /// <summary>
                /// Retrieve information (+ metadata) of a single token type (template) from one of your contracts
                /// [/api/contracts/:contractId/token-types/:tokenTypeId/metadata]
                /// </summary>
                /// <param name="contractId">Contract ID related to the token type (nft-template)</param>
                /// <param name="tokenTypeId">ID of the token type (nft-template)</param>
                /// <returns>NFT token type (template) Information including metadata</returns>
                public static VyTask<VyNonFungibleToken> GetTokenTypeMetadata(int contractId, int tokenTypeId)
                {
                    var reqData = RequestData.Get($"/api/contracts/{contractId}/token-types/{tokenTypeId}/metadata", _apiEndpoint);
                    return Request<VyNonFungibleToken>(reqData);
                }

                //todo: check name/data mangling between VyNonFungibleToken & VyTokenType (including metadata)...
                /// <summary>
                /// Retrieve information (+ metadata) of a single token from one of your contracts
                /// [/api/contracts/:contractId/tokens/:tokenId/metadata]
                /// </summary>
                /// <param name="contractId">Contract ID related to the token</param>
                /// <param name="tokenId">ID of the token</param>
                /// <returns>NFT token Information including metadata</returns>
                public static VyTask<VyNonFungibleToken> GetTokenMetadata(int contractId, int tokenId)
                {
                    var reqData = RequestData.Get($"/api/contracts/{contractId}/tokens/{tokenId}/metadata", _apiEndpoint);
                    return Request<VyNonFungibleToken>(reqData);
                }

                /// <summary>
                /// Retrieve all the tokens related to a specific token-type (template)
                /// [/api/minter/contracts/:contractId/token-types/:tokenTypeId/tokens]
                /// </summary>
                /// <param name="applicationId">Your personal application ID</param>
                /// <param name="contractId">Contract ID related to the token-type (nft-template)</param>
                /// <param name="tokenTypeId">ID of the token-type (nft-template)</param>
                /// <returns>List of all the Tokens associated with the given token-type (nft-template)</returns>
                public static VyTask<VyNonFungibleToken[]> GetTokensForType(int contractId, int tokenTypeId)
                {
                    var reqData = RequestData.Get($"/api/minter/contracts/{contractId}/token-types/{tokenTypeId}/tokens", _apiEndpoint);
                    return Request<VyNonFungibleToken[]>(reqData);
                }
            }

            //todo
            //public static class Market
            //{

            //}
        }
    }
}