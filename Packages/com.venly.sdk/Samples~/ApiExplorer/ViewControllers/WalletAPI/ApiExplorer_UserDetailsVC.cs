using System;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Wallet;

public class ApiExplorer_UserDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_USER = "user";
    public const string DATAKEY_USER_ID = "user-id";

    //DATA
    private VyUserDto _user;
    private VyWalletDto[] _userWallets;

    //UI BIND
    [UIBind("lst-wallets")] private VyControl_WalletListView _lstWallets;

    public ApiExplorer_UserDetailsVC() :
        base(eApiExplorerViewId.WalletApi_UserDetails)
    { }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-add-wallet", OnClick_AddWallet);
    }

    protected override void OnActivate()
    {
        if (_lstWallets != null)
        {
            _lstWallets.OnItemSelected += OnClick_Wallet;
        }
        else ViewManager.HandleException(new Exception("lstWallets is null"));

        if (!TryGetBlackboardData(out _user, DATAKEY_USER, ApiExplorer_GlobalKeys.DATA_User))
        {
            NoDataRefresh = false;
        }
    }

    protected override async Task OnRefreshData()
    {
        string userId = null;
        if (_user != null) userId = _user.Id;
        else if (!TryGetBlackboardData(out userId, DATAKEY_USER_ID))
        {
            ViewManager.HandleException(new ArgumentException("Failed to retrieve the user id..."));
            return;
        }

        //Retrieve User Data
        //--------------------
        ViewManager.Loader.Show("Retrieving User Info...");
        var result = await VenlyAPI.Wallet.GetUser(userId);
        ViewManager.Loader.Hide();

        if (!result.Success)
        {
            ViewManager.HandleException(result.Exception);
            return;
        }

        _user = result.Data;
        SetBlackboardData(DATAKEY_USER, _user); //Update Blackboard (contains Balance now)

        //Retrieve Wallets 
        //----------------
        ViewManager.Loader.Show("Retrieving Wallets...");
        var walletResult = await VenlyAPI.Wallet.GetWallets(VyQuery_GetWallets.Create().UserId(_user.Id));
        ViewManager.Loader.Hide();

        if (!walletResult.Success)
        {
            ViewManager.HandleException(result.Exception);
            _userWallets = Array.Empty<VyWalletDto>();
        }
        else
        {
            _userWallets = walletResult.Data;
        }
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_user, DATAKEY_USER)) return;
        if (!ValidateData(_userWallets, "user_wallets")) return;

        //Refresh UI Elements
        SetLabel("lbl-user-id", _user.Id);
        SetLabel("lbl-user-reference", _user.Reference);
        SetLabel("lbl-user-created", _user.CreatedAt?.ToString()??"N/A");


        _lstWallets?.SetItemSource(_userWallets);
    }
    #endregion

    #region EVENTS
    private async void OnClick_Wallet(VyWalletDto wallet)
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_WalletDetails, ApiExplorer_WalletDetailsVC.DATAKEY_WALLET, wallet);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_WalletDetails, ViewId, false);
    }

    private void OnClick_AddWallet()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_CreateWallet, ApiExplorer_CreateWalletVC.DATAKEY_USER, _user);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_CreateWallet, ViewId, true);
    }
    #endregion
}
