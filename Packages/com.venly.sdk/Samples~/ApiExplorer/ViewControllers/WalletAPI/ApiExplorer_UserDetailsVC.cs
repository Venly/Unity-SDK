using System;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Wallet;

[SampleViewMeta(eApiExplorerViewId.WalletApi_UserDetails, "User Details")]
public class ApiExplorer_UserDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public static readonly BlackboardKey<VyUserDto> KEY_User = new BlackboardKey<VyUserDto>("user");
    public static readonly BlackboardKey<string> KEY_UserId = new BlackboardKey<string>("user-id");

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

        if (!TryGet(KEY_User, out _user))
        {
            NoDataRefresh = false;
        }
    }

    protected override async Task OnRefreshData()
    {
        string userId = null;
        if (_user != null) userId = _user.Id;
        else if (!TryGet(KEY_UserId, out userId))
        {
            ViewManager.HandleException(new ArgumentException("Failed to retrieve the user id..."));
            return;
        }

        //Retrieve User Data
        //--------------------
        using (ViewManager.BeginLoad("Retrieving User Info..."))
        {
            var result = await VenlyAPI.Wallet.GetUser(userId);
            if (!result.Success)
            {
                ViewManager.HandleException(result.Exception);
                return;
            }
            _user = result.Data;
            Set(KEY_User, _user); //Update Blackboard (contains Balance now)
        }

        //Retrieve Wallets 
        //----------------
        using (ViewManager.BeginLoad("Retrieving Wallets..."))
        {
            var walletResult = await VenlyAPI.Wallet.GetWallets(VyQuery_GetWallets.Create().UserId(_user.Id));
            if (!walletResult.Success)
            {
                ViewManager.HandleException(walletResult.Exception);
                _userWallets = Array.Empty<VyWalletDto>();
            }
            else
            {
                _userWallets = walletResult.Data;
            }
        }
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_user, "user")) return;
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
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_WalletDetails, ApiExplorer_WalletDetailsVC.KEY_Wallet, wallet);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_WalletDetails, ViewId, false);
    }

    private void OnClick_AddWallet()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_CreateWallet, ApiExplorer_CreateWalletVC.KEY_User, _user);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_CreateWallet, ViewId, true);
    }
    #endregion
}
