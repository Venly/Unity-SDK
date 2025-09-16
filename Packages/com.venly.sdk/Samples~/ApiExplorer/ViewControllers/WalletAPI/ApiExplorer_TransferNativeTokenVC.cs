using System;
using System.Globalization;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Wallet;

public class ApiExplorer_TransferNativeTokenVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_WALLET = "wallet";

    //DATA
    private VyWalletDto _sourceWallet;
    private VyUserDto _sourceUser;

    public ApiExplorer_TransferNativeTokenVC() : 
        base(eApiExplorerViewId.WalletApi_TransferNativeToken) { }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-select-source", OnClick_SelectSource);
        BindButton("btn-select-target", OnClick_SelectTarget);
        BindButton("btn-transfer", OnClick_Transfer);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
        ShowNavigateHome = false;

        //Check if Source Is Set
        TryGetBlackboardData(out _sourceWallet, DATAKEY_WALLET);
        ToggleElement("btn-select-source", _sourceWallet == null);
        UpdateSourceWallet(_sourceWallet);
    }

    private void UpdateSourceWallet(VyWalletDto sourceWallet)
    {
        _sourceWallet = sourceWallet;

        SetLabel("lbl-source-wallet", _sourceWallet == null?"select wallet":_sourceWallet.Id);
        SetLabel("lbl-token", _sourceWallet == null?"select wallet":_sourceWallet.Balance.Symbol);
        SetLabel("lbl-balance", _sourceWallet == null?"select wallet":_sourceWallet.Balance.Balance?.ToString(CultureInfo.InvariantCulture));

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
                    ViewManager.Loader.Show("Retrieving Associated User...");
                    _sourceUser = user;
                    ToggleElement("btn-transfer", true);
                })
                .OnFail(ex => ViewManager.HandleException(ex))
                .Finally(() => ViewManager.Loader.Hide());
        }
    }

    protected override void OnDeactivate()
    {
       ClearBlackboardData("sourceWallet");

       _sourceWallet = null;
    }
    #endregion

    #region EVENTS
    private async void OnClick_SelectSource()
    {
        var selection = await ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewWallets, "Select Wallet");
        if(!selection.Success) return;

        //Update Wallet
        var selectedWallet = selection.Data as VyWalletDto; //Will have no Balance
        ViewManager.Loader.Show("Refreshing Wallet Details...");
        var result = await VenlyAPI.Wallet.GetWallet(selectedWallet.Id);
        ViewManager.Loader.Hide();

        if (result.Success) UpdateSourceWallet(result.Data);
        else ViewManager.HandleException(result.Exception);
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
        if (!ValidateData(_sourceWallet, "sourceWallet")) return;
        if (!ValidateData(_sourceUser, "sourceUser")) return;
        if (!ValidateInput("txt-pincode")) return;
        if (!ValidateInput("txt-target-address")) return;
        if (!ValidateInput<float>("txt-amount")) return;

        var reqParams = new VyCreateTransferTransactionRequest()
        {
            Chain = _sourceWallet.Chain,
            WalletId = _sourceWallet?.Id,
            To = GetValue("txt-target-address"),
            Value = GetValue<float>("txt-amount"),
            Data = "Venly Api Explorer Transaction (Native Token)"

        };

        if (!_sourceUser.TryGetPinAuth(GetValue("txt-pincode"), out var userAuth))
        {
            ViewManager.HandleException(new Exception("Failed to retrieve UserAuth"));
            return;
        }

        ViewManager.Loader.Show("Transferring...");
        VenlyAPI.Wallet.CreateTransaction_Transfer(reqParams, userAuth)
            .OnSuccess(transferInfo =>
            {
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, ApiExplorer_TransactionDetailsVC.DATAKEY_TXHASH, transferInfo.TransactionHash);
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransactionDetails, ApiExplorer_TransactionDetailsVC.DATAKEY_TXCHAIN, _sourceWallet.Chain);
                ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransactionDetails, CurrentBackTarget);
            })
            .OnFail(ViewManager.HandleException)
            .Finally(ViewManager.Loader.Hide);
    }
    #endregion
}
