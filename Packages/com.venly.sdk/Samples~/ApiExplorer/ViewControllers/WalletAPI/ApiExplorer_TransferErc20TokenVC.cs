using System;
using System.Globalization;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Wallet;

public class ApiExplorer_TransferErc20TokenVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public static readonly BlackboardKey<VyWalletDto> KEY_Wallet = new BlackboardKey<VyWalletDto>("wallet");
    public static readonly BlackboardKey<VyErc20TokenDto> KEY_Token = new BlackboardKey<VyErc20TokenDto>("token");

    //DATA
    private VyWalletDto _sourceWallet;
    private VyUserDto _sourceUser;
    private VyErc20TokenDto _sourceToken;

    //UI
    [UIBind("btn-select-source-wallet")] private Button _btnSelectSourceWallet;
    [UIBind("btn-select-source-token")] private Button _btnSelectSourceToken;

    public ApiExplorer_TransferErc20TokenVC() : 
        base(eApiExplorerViewId.WalletApi_TransferErc20Token) { }

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

        //Check if Source Is Set
        TryGet(KEY_Wallet, out _sourceWallet);
        ToggleElement("btn-select-source-wallet", _sourceWallet == null);
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
        _sourceWallet = sourceWallet;

        SetLabel("lbl-source-wallet", _sourceWallet == null?"select wallet":_sourceWallet.Id);

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

    private void UpdateSourceToken(VyErc20TokenDto sourceToken)
    {
        _sourceToken = sourceToken;

        if (_sourceToken == null)
        {
            SetLabel("lbl-source-token", "select token");
        }
        else
        {
            SetLabel("lbl-source-token", _sourceToken.Name);
        }

        _btnSelectSourceToken.ToggleDisplay(_sourceWallet != null);
    }

    protected override void OnDeactivate()
    {
        ClearBlackboardData();
    }
    #endregion

    private void OnClick_SelectSourceToken()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_ViewErc20Tokens, ApiExplorer_ViewTokensBaseVC<VyErc20TokenDto, VyControl_Erc20TokenListView, VyControl_Erc20TokenListItem>.KEY_SourceWallet, _sourceWallet);
        ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewErc20Tokens, "Select Token")
            .OnComplete(result =>
            {
                if (result.Success)
                {
                    var token = result.Data as VyErc20TokenDto;
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
        if (!ValidateData(_sourceUser, "sourceUser")) return;
        if (!ValidateData(_sourceToken, "sourceToken")) return;
        if (!ValidateInput("txt-pincode")) return;
        if (!ValidateInput("txt-target-address")) return;
        if (!ValidateInput<float>("txt-amount")) return;

        var reqParams = new VyBuildTokenTransferTransactionRequest()
        {
            Chain = _sourceWallet.Chain,
            WalletId = _sourceWallet?.Id,
            TokenAddress = _sourceToken.TokenAddress,
            To = GetValue("txt-target-address"),
            Value = GetValue<float>("txt-amount")
        };

        if (!_sourceUser.TryGetPinAuth(GetValue("txt-pincode"), out var userAuth))
        {
            ViewManager.HandleException(new Exception("Failed to retrieve UserAuth"));
            return;
        }


        VenlyAPI.Wallet.TransferErc20Token(reqParams, userAuth)
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
