//Sample View Manager
public class ApiExplorer_ViewManager : SampleViewManager<eApiExplorerViewId>
{
    public override string GetTitle(eApiExplorerViewId viewId)
    {
        switch (viewId)
        {
            case eApiExplorerViewId.Unknown: return "Unknown";
            case eApiExplorerViewId.Count: return "Ignore";

            case eApiExplorerViewId.Landing_DevMode: return "Venly SDK | API Explorer";
            case eApiExplorerViewId.Landing_RuntimeMode: return "Venly SDK | API Explorer";
            case eApiExplorerViewId.Landing_Auth: return "Venly SDK | API Explorer";

            case eApiExplorerViewId.Auth_Login: return "Login User";
            case eApiExplorerViewId.Auth_Create: return "Create User";
            case eApiExplorerViewId.Auth_UserPortal: return "User Portal";

            case eApiExplorerViewId.Shared_Erc1155TokenDetails: return "Erc1155 Token";
            case eApiExplorerViewId.Shared_Erc20TokenDetails: return "Erc20 Token";

            case eApiExplorerViewId.Main_WalletApi: return "Wallet API";
            case eApiExplorerViewId.WalletApi_ViewWallets: return "View Wallets";
            case eApiExplorerViewId.WalletApi_CreateWallet: return "Create Wallet";
            case eApiExplorerViewId.WalletApi_WalletDetails: return "Wallet Details";
            case eApiExplorerViewId.WalletApi_ViewNfts: return "Erc1155 Tokens";
            case eApiExplorerViewId.WalletApi_ViewErc20Tokens: return "Erc20 Tokens";
            case eApiExplorerViewId.WalletApi_TransferNativeToken: return "Transfer Native Token";
            case eApiExplorerViewId.WalletApi_ViewWalletEvents: return "Wallet Events";
            case eApiExplorerViewId.WalletApi_TransferErc20Token: return "Transfer Erc20 Token";
            case eApiExplorerViewId.WalletApi_TransferNft: return "Transfer Erc1155 Token";
            case eApiExplorerViewId.WalletApi_TransactionDetails: return "Transaction Details";
            case eApiExplorerViewId.WalletApi_CreateUser: return "Create User";
            case eApiExplorerViewId.WalletApi_ViewUsers: return "View Users";
            case eApiExplorerViewId.WalletApi_UserDetails: return "User Details";

            case eApiExplorerViewId.Main_TokenApi: return "Token API";
            case eApiExplorerViewId.TokenApi_ViewErc1155Contracts: return "View Erc1155 Contracts";
            case eApiExplorerViewId.TokenApi_Erc1155ContractDetails: return "Erc1155 Contract Details";
            case eApiExplorerViewId.TokenApi_ViewErc1155TokenTypes: return "View Erc1155 Token Types";
            case eApiExplorerViewId.TokenApi_Erc1155TokenTypeDetails: return "Erc1155 Token Type Details";
            case eApiExplorerViewId.TokenApi_MintErc1155Token: return "Mint Erc1155 Token";

            default: return "Untitled";
        }
    }
}

//Sample View Id
public enum eApiExplorerViewId : uint
{
    Unknown = 0,

    Landing_DevMode = 1,
    Landing_RuntimeMode = 2,
    Landing_Auth = 3,

    Auth_Login = 4,
    Auth_Create = 5,
   
    Shared_Erc1155TokenDetails = 6,
    Shared_Erc20TokenDetails = 7,

    Main_WalletApi = 8,
    WalletApi_ViewWallets = 9,
    WalletApi_CreateWallet = 10,
    WalletApi_WalletDetails = 11,
    WalletApi_ViewNfts = 12,
    WalletApi_ViewErc20Tokens = 13,
    WalletApi_ViewWalletEvents = 14,
    WalletApi_TransferNativeToken = 15,
    WalletApi_TransferErc20Token = 16,
    WalletApi_TransferNft = 17,
    WalletApi_TransactionDetails = 18,
    WalletApi_CreateUser = 19,
    WalletApi_ViewUsers = 20,
    WalletApi_UserDetails = 21,

    Main_TokenApi = 30,
    TokenApi_ViewErc1155Contracts = 31,
    TokenApi_Erc1155ContractDetails = 32,
    TokenApi_ViewErc1155TokenTypes = 33,
    TokenApi_Erc1155TokenTypeDetails = 34,
    TokenApi_MintErc1155Token = 35,

    Auth_UserPortal = 40,

    Count
}

public static class ApiExplorer_GlobalKeys
{
    public const string DATA_AllWalletsCached = "all-wallets-cache";
    public const string DATA_CachedUsers = "cached-users";
    public const string DATA_CachedErc1155Contracts = "cached-erc1155-contracts";
    public const string DATA_Erc1155Contract = "erc1155-contract";
    public const string DATA_User = "provider-user";
}
