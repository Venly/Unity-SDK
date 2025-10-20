using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Venly;
using Venly.Core;
using Venly.Models.Shared;
using Venly.Models.Wallet;
using Venly.Utils;

public class ApiExplorer_WalletDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public static readonly BlackboardKey<VyWalletDto> KEY_Wallet = new BlackboardKey<VyWalletDto>("wallet");

    //DATA
    private VyWalletDto _wallet;
    private VyErc20TokenDto[] _cryptoTokens;
    private VyNftDto[] _multiTokens;
    private VyNftDto[] _multiTokens_FT;
    private VyNftDto[] _multiTokens_NFT;
    private VyWalletEventDto[] _walletEvents;

    //UI-BIND
    [UIBind("btn-view-user")] private Button _btnViewUser;

    public ApiExplorer_WalletDetailsVC() :
        base(eApiExplorerViewId.WalletApi_WalletDetails)
    { }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        BindButton("btn-view-user", OnClick_ViewUser);

        BindButton("btn-cryptotokens", OnClick_ShowCryptoTokens);
        BindButton("btn-multitokens-f", OnClick_ShowMultiTokens_FT);
        BindButton("btn-multitokens-nf", OnClick_ShowMultiTokens_NFT);
        BindButton("btn-events", OnClick_ShowWalletEvents);

        BindButton("btn-archive", OnClick_Archive);
        BindButton("btn-transfer", OnClick_Transfer);
        BindButton("btn-transfer-erc20", OnClick_TransferErc20);
        BindButton("btn-transfer-nft", OnClick_TransferNft);

        //Only show Archive Button when DevMode is active
        ToggleElement("btn-archive", VenlySettings.BackendProvider == eVyBackendProvider.DevMode);
    }

    protected override void OnActivate()
    {
        TryGet(KEY_Wallet, out _wallet);
    }

    protected override async Task OnRefreshData()
    {
        //Retrieve Wallet Data
        //--------------------
        using (ViewManager.BeginLoad("Retrieving Wallet Info..."))
        {
            var result = await VenlyAPI.Wallet.GetWallet(_wallet.Id);
            if (!result.Success)
            {
                ViewManager.HandleException(result.Exception);
                return;
            }
            _wallet = result.Data;
            Set(KEY_Wallet, _wallet); //Update Blackboard (contains Balance now)
        }

        //Retrieve MultiToken Balances
        //----------------------------
        using (ViewManager.BeginLoad("Retrieving ERC1155/721 Tokens..."))
        {
            var nftResult = await VenlyAPI.Wallet.GetNfts(_wallet.Id);
            if (!nftResult.Success)
            {
                ViewManager.HandleException(nftResult.Exception);
                _multiTokens = Array.Empty<VyNftDto>();
                _multiTokens_FT = _multiTokens;
                _multiTokens_NFT = _multiTokens;
            }
            else
            {
                _multiTokens = nftResult.Data;
                _multiTokens_FT = _multiTokens.Where(t => t.Fungible??false).ToArray();
                _multiTokens_NFT = _multiTokens.Where(t => !t.Fungible??false).ToArray();
            }
        }

        //Retrieve CryptoToken Balances
        //-----------------------------
        using (ViewManager.BeginLoad("Retrieving ERC20 Tokens..."))
        {
            var ftResult = await VenlyAPI.Wallet.GetErc20Tokens(_wallet.Id);
            if (!ftResult.Success)
            {
                ViewManager.HandleException(ftResult.Exception);
                _cryptoTokens = Array.Empty<VyErc20TokenDto>();
            }
            else _cryptoTokens = ftResult.Data;
        }

        //Retrieve Events
        //---------------
        using (ViewManager.BeginLoad("Retrieving Wallet Events..."))
        {
            var eventsResult = await VenlyAPI.Wallet.GetWalletEvents(_wallet.Id);
            if (!eventsResult.Success)
            {
                ViewManager.HandleException(eventsResult.Exception);
                _walletEvents = Array.Empty<VyWalletEventDto>();
            }
            else _walletEvents = eventsResult.Data;
        }

    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_wallet, "wallet")) return;
        if (!ValidateData(_cryptoTokens, "cryptoTokens")) return;
        if (!ValidateData(_multiTokens_FT, "multiTokens_ft")) return;
        if (!ValidateData(_multiTokens_NFT, "multiTokens_nft")) return;
        if (!ValidateData(_walletEvents, "events")) return;

        //Refresh UI Elements
        SetLabel("btn-transfer", $"Transfer {_wallet.Balance.Symbol}");
        SetLabel("lbl-wallet-address", _wallet.Address);
        SetLabel("lbl-wallet-id", _wallet.Id);
        SetLabel("lbl-wallet-chain", _wallet.Chain.HasValue ? _wallet.Chain.Value.GetMemberName() : "Unknown");
        SetLabel("lbl-wallet-description", _wallet.Description);
        SetLabel("lbl-wallet-identifier", _wallet.Identifier);
        SetLabel("lbl-wallet-balance", $"{_wallet.Balance.Balance} {_wallet.Balance.Symbol}");

        var userLinked = !string.IsNullOrEmpty(_wallet.UserId);
        ToggleElement("btn-view-user", userLinked);
        SetLabel("lbl-user-id", userLinked?_wallet.UserId:"No User Linked");

        //Token Data
        SetLabel("lbl-cryptotoken-amount", $"{_cryptoTokens.Length} Token(s)");
        SetLabel("lbl-multitoken-ft-amount", $"{_multiTokens_FT.Length} Token(s)");
        SetLabel("lbl-multitoken-nft-amount", $"{_multiTokens_NFT.Length} Token(s)");
        SetLabel("lbl-wallet-event-amount", $"{_walletEvents.Length} Event(s)");

        ToggleElement("btn-transfer-erc20", userLinked && _cryptoTokens.Length > 0);
        ToggleElement("btn-transfer-nft", userLinked && _multiTokens.Length > 0);
        ToggleElement("btn-transfer", userLinked);

        //Archived State
        View.rootVisualElement.SetButtonLabel("btn-archive", _wallet.Archived??false ? "Unarchive" : "Archive");
        View.rootVisualElement.SetDisplay("lbl-archived", _wallet.Archived??false);
    }
    #endregion

    #region EVENTS
    private async void OnClick_ViewUser()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_UserDetails, ApiExplorer_UserDetailsVC.KEY_UserId, _wallet.UserId);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_UserDetails, ViewId, false);
    }

    private async void OnClick_Archive()
    {
        VyTaskResult<VyWalletDto> result = null;
        if (!_wallet.Archived.Value)
        {
            using (ViewManager.BeginLoad("Archiving Wallet.."))
            {
                result = await VenlyAPI.Wallet.ArchiveWallet(_wallet.Id);
            }
        }
        else
        {
            using (ViewManager.BeginLoad("Unarchiving Wallet.."))
            {
                result = await VenlyAPI.Wallet.UnarchiveWallet(_wallet.Id);
            }
        }

        if (result.Success)
        {
            _wallet = result.Data;
            Refresh(false);

            ViewManager.ClearGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_CachedWallets);
        }
        else ViewManager.HandleException(result.Exception);
    }

    private void OnClick_Transfer()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransferNativeToken, ApiExplorer_TransferNativeTokenVC.KEY_Wallet, _wallet);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransferNativeToken, ViewId, false);
    }

    private void OnClick_TransferNft()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransferNft, ApiExplorer_TransferNftVC.KEY_Wallet, _wallet);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransferNft, ViewId, false);
    }

    private void OnClick_TransferErc20()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransferErc20Token, ApiExplorer_TransferErc20TokenVC.KEY_Wallet, _wallet);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransferErc20Token, ViewId, false);
    }

    private void OnClick_ShowCryptoTokens()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewErc20Tokens, ApiExplorer_ViewTokensBaseVC<VyErc20TokenDto, VyControl_Erc20TokenListView, VyControl_Erc20TokenListItem>.KEY_SourceWallet, _wallet);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewErc20Tokens, ApiExplorer_ViewTokensBaseVC<VyErc20TokenDto, VyControl_Erc20TokenListView, VyControl_Erc20TokenListItem>.KEY_TokenList, _cryptoTokens);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_ViewErc20Tokens, ViewId, false);
    }

    private void OnClick_ShowMultiTokens_FT()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewNfts, ApiExplorer_ViewTokensBaseVC<VyNftDto, VyControl_NftListView, VyControl_NftListItem>.KEY_SourceWallet, _wallet);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewNfts, ApiExplorer_ViewTokensBaseVC<VyNftDto, VyControl_NftListView, VyControl_NftListItem>.KEY_TokenList, _multiTokens_FT);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_ViewNfts, ViewId, false);
    }

    private void OnClick_ShowMultiTokens_NFT()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewNfts, ApiExplorer_ViewTokensBaseVC<VyNftDto, VyControl_NftListView, VyControl_NftListItem>.KEY_SourceWallet, _wallet);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewNfts, ApiExplorer_ViewTokensBaseVC<VyNftDto, VyControl_NftListView, VyControl_NftListItem>.KEY_TokenList, _multiTokens_NFT);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_ViewNfts, ViewId, false);
    }

    private void OnClick_ShowWalletEvents()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewWalletEvents, ApiExplorer_ViewWalletEventsVC.KEY_Events, _walletEvents);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_ViewWalletEvents, ViewId, false);
    }
    #endregion
}
