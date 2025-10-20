using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venly;
using Venly.Models.Wallet;

[SampleViewMeta(eApiExplorerViewId.WalletApi_ViewWallets, "View Wallets")]
public class ApiExplorer_ViewWalletsVC : SampleViewBase<eApiExplorerViewId>
{
    //Flag
    public bool EnforceUserId = true;

    //DATA
    private List<VyWalletDto> _walletList = null;

    //UI
    [UIBind("lst-wallets")] private VyControl_WalletListView _lstWallets;

    public ApiExplorer_ViewWalletsVC() :
        base(eApiExplorerViewId.WalletApi_ViewWallets) { }

    #region DATA & UI
    protected override void OnActivate()
    {
        _walletList = null;
        _lstWallets.OnItemSelected += OnClick_Wallet;

        //Check for Cached Wallets
        if (ViewManager.TryGetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_CachedWallets, out var cachedWallets))
        {
            _walletList = cachedWallets;
            NoDataRefresh = true;
        }
    }

    protected override async Task OnRefreshData()
    {
        using (ViewManager.BeginLoad("Retrieving Wallets..."))
        {
            var query = VyQuery_GetWallets
                .Create()
                .IncludeBalance(false)
                .IncludeUsers(true);
            var result = await VenlyAPI.Wallet.GetWallets(query);

            if (result.Success)
            {
                _walletList = result.Data.OrderByDescending(w => w.CreatedAt).ToList();

                if (EnforceUserId)
                    _walletList = _walletList.Where(w => !string.IsNullOrEmpty(w.UserId)).ToList();

                //Store to global
                ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_CachedWallets, _walletList);
            }
            else ViewManager.HandleException(result.Exception);
        }
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_walletList, "walletList")) return;

        _lstWallets.SetItemSource(_walletList);
    }
    #endregion

    #region EVENTS
    private void OnClick_Wallet(VyWalletDto wallet)
    {
        if (IsSelectionMode)
        {
            FinishSelection(wallet);
            return;
        }

        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_WalletDetails, ApiExplorer_WalletDetailsVC.KEY_Wallet, wallet);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_WalletDetails);
    }
    #endregion

    protected override void OnDeactivate()
    {
        if (_lstWallets != null) _lstWallets.OnItemSelected -= OnClick_Wallet;
    }
}
