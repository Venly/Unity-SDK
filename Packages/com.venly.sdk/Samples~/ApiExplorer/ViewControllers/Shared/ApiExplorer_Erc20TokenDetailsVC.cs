using UnityEngine.UIElements;
using Venly.Models.Shared;
using Venly.Models.Wallet;

public class ApiExplorer_Erc20TokenDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA
    private VyErc20TokenDto _token;

    public ApiExplorer_Erc20TokenDetailsVC() : 
        base(eApiExplorerViewId.Shared_Erc20TokenDetails)
    {
    }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        BindButton("btn-transfer", OnClick_Transfer);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
        ShowNavigateHome = false;

        //Retrieve Token from Blackboard (should be set by calling view)
        TryGet(ApiExplorer_TokenDetailsDataKeys.KEY_Erc20Token, out _token);
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_token, "token")) return;

        SetLabel("lbl-token-name", _token.Name);
        SetLabel("lbl-token-symbol", _token.Symbol);
        SetLabel("lbl-token-balance", _token.Balance.ToString());
        SetLabel("lbl-token-decimals", _token.Decimals.ToString());
        SetLabel("lbl-token-transferable", _token.Transferable??false ? "YES" : "NO");
        SetLabel("lbl-token-address", _token.TokenAddress);
    }
    #endregion

    #region EVENTS
    private void OnClick_Transfer()
    {
        if (!TryGet(ApiExplorer_TokenDetailsDataKeys.KEY_WALLET, out VyWalletDto wallet)) return;
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransferErc20Token, ApiExplorer_TransferErc20TokenVC.KEY_Wallet, wallet);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransferErc20Token, ApiExplorer_TransferErc20TokenVC.KEY_Token, _token);

        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransferErc20Token);
    }
    #endregion
}
