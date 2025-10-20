using System;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Token;
using Venly.Models.Wallet;

[SampleViewMeta(eApiExplorerViewId.WalletApi_TransferNft, "Transfer NFT")] 
public class ApiExplorer_TransferNftVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public static readonly BlackboardKey<VyWalletDto> KEY_Wallet = new ("wallet");
    public static readonly BlackboardKey<VyNftDto> KEY_Token = new ("token");

    //DATA
    private VyWalletDto _sourceWallet;
    private VyUserDto _sourceUser;
    private VyNftDto _sourceToken;

    //UI
    [UIBind("btn-select-source-wallet")] private Button _btnSelectSourceWallet;
    [UIBind("btn-select-source-token")] private Button _btnSelectSourceToken;


    public ApiExplorer_TransferNftVC() :
        base(eApiExplorerViewId.WalletApi_TransferNft)
    { }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-select-source-wallet", OnClick_SelectSourceWallet);
        BindButton("btn-select-source-token", OnClick_SelectSourceToken);
        BindButton("btn-select-target", OnClick_SelectTarget);
        BindButton("btn-transfer", OnClick_Transfer);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
        ShowNavigateHome = false;

        _sourceWallet = null;
        _sourceToken = null;

        //Check if Source Is Set
        TryGet(KEY_Wallet, out _sourceWallet);
        ToggleElement("btn-select-source-wallet", _sourceWallet == null);
        UpdateSourceWallet(_sourceWallet);

        _btnSelectSourceWallet.ToggleDisplay(_sourceWallet == null);
        UpdateSourceWallet(_sourceWallet);

        //Check if Token is Set
        if (_sourceWallet != null)
        {
            TryGet(KEY_Token, out _sourceToken);
            _btnSelectSourceToken.ToggleDisplay(_sourceToken == null);
            UpdateSourceToken(_sourceToken);
        }
    }

    private void UpdateSourceWallet(VyWalletDto sourceWallet)
    {
        if (_sourceWallet != sourceWallet) //new wallet, reset token
        {
            _sourceToken = null;
        }

        _sourceWallet = sourceWallet;

        if (_sourceWallet == null)
        {
            SetLabel("lbl-source-wallet", "select wallet");
            SetLabel("lbl-source-token", "select wallet");
            SetLabel("lbl-type", "select wallet");
        }
        else
        {
            SetLabel("lbl-source-wallet", _sourceWallet.Id);
            UpdateSourceToken(_sourceToken);
        }

        //Hide Select Token button if no wallet is selected yet
        _btnSelectSourceToken.ToggleDisplay(_sourceWallet != null);

        //Retrieve UserAuth
        if (_sourceWallet != null)
        {
            _sourceUser = null;
            if (string.IsNullOrEmpty(_sourceWallet.UserId))
            {
                ViewManager.HandleException(new Exception("Wallet has no associated user..."));
                ToggleElement("btn-transfer", false);
                return;
            }

            VenlyAPI.Wallet.GetUser(_sourceWallet.UserId)
                .OnSuccess(user =>
                {
                    using (ViewManager.BeginLoad("Retrieving Associated User..."))
                    {
                        _sourceUser = user;
                        ToggleElement("btn-transfer", true);
                    }
                })
                .OnFail(ex => ViewManager.HandleException(ex))
                .Finally(() => { /* scope handles loader */ });
        }
    }

    private void UpdateSourceToken(VyNftDto sourceToken)
    {
        _sourceToken = sourceToken;

        if (_sourceToken == null)
        {
            SetLabel("lbl-source-token", "select token");
            SetLabel("lbl-type", "select token");
        }
        else
        {
            var tokenName = $"{_sourceToken.Contract.Name} (#{_sourceToken.Id})";
            SetLabel("lbl-source-token", tokenName);
            SetLabel("lbl-type", _sourceToken.Contract.Type);
        }

        ToggleElement("txt-amount", _sourceToken?.Fungible??true);
    }

    protected override void OnDeactivate()
    {
        ClearBlackboardData();
    }
    #endregion

    private void OnClick_SelectSourceToken()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewNfts, ApiExplorer_ViewTokensBaseVC<VyNftDto, VyControl_NftListView, VyControl_NftListItem>.KEY_SourceWallet, _sourceWallet);
        ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewNfts, "Select Token")
            .OnComplete(result =>
            {
                if (result.Success)
                {
                    var token = result.Data as VyNftDto;
                    UpdateSourceToken(token);
                }
            });
    }

    private void OnClick_SelectSourceWallet()
    {
        ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewWallets, "Select Wallet")
            .OnComplete(result =>
            {
                if (result.Success)
                {
                    var wallet = result.Data as VyWalletDto;
                    UpdateSourceWallet(wallet);
                }
            });
    }

    private void OnClick_SelectTarget()
    {
        ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewWallets, "Select Wallet")
            .OnComplete(result =>
            {
                if (result.Success)
                {
                    var wallet = result.Data as VyWalletDto;
                    SetLabel("txt-target-address", wallet.Address);
                }
            });
    }

    private void OnClick_Transfer()
    {
        //Validate
        if (!ValidateData(_sourceWallet, "sourceWallet")) return;
        if (!ValidateData(_sourceToken, "sourceToken")) return;
        if (!ValidateData(_sourceUser, "sourceUser")) return;
        if (!ValidateInput("txt-pincode")) return;
        if (!ValidateInput("txt-target-address")) return;

        if(_sourceToken?.Fungible ?? true)
            if (!ValidateInput<int>("txt-amount")) return;

        //Execute
        var reqParams = new VyBuildNftTransferTransactionRequest()
        {
            Chain = _sourceWallet?.Chain,
            WalletId = _sourceWallet?.Id,
            TokenAddress = _sourceToken.Contract.Address,
            TokenId = _sourceToken.Id,
            To = GetValue("txt-target-address"),
            Amount = _sourceToken.Fungible??false?GetValue<int>("txt-amount"):null
        };

        if (!_sourceUser.TryGetPinAuth(GetValue("txt-pincode"), out var userAuth))
        {
            ViewManager.HandleException(new Exception("Failed to retrieve UserAuth"));
            return;
        }

        //using (ViewManager.BeginLoad("Transferring..."))
        //{
        //    var result = await VenlyAPI.Wallet.TransferNFT(reqParams, userAuth);
        //    if (!result.Success) ViewManager.HandleException(result.Exception);
        //    else
        //    {
        //        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, ApiExplorer_TransactionDetailsVC.KEY_TxHash, result.Data.TransactionHash);
        //        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, ApiExplorer_TransactionDetailsVC.KEY_TxChain, _sourceWallet.Chain.Value);
        //        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransactionDetails, CurrentBackTarget);
        //    }
        //}

        VenlyAPI.Wallet.TransferNFT(reqParams, userAuth)
            .WithLoader(ViewManager, "Transferring...")
            .OnSuccess(transferInfo =>
            {
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, ApiExplorer_TransactionDetailsVC.KEY_TxHash, transferInfo.TransactionHash);
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, ApiExplorer_TransactionDetailsVC.KEY_TxChain, _sourceWallet.Chain.Value);
                ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransactionDetails, CurrentBackTarget);
            })
            .OnFail(ViewManager.HandleException);
    }
}
