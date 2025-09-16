using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venly;
using Venly.Models.Wallet;

public class ApiExplorer_ViewUsersVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA
    private List<VyUserDto> _userList = null;

    //UI
    [UIBind("lst-users")] private VyControl_UserListView _lstUsers;

    public ApiExplorer_ViewUsersVC() :
        base(eApiExplorerViewId.WalletApi_ViewUsers) { }

    #region DATA & UI
    protected override void OnActivate()
    {
        _userList = null;
        _lstUsers.OnItemSelected += OnClick_User;

        //Check for Cached Wallets
        if (TryGetBlackboardData(out VyUserDto[] resultArr, globalKey: ApiExplorer_GlobalKeys.DATA_CachedUsers))
        {
            _userList = resultArr.ToList();
            NoDataRefresh = true;
        }
    }

    protected override async Task OnRefreshData()
    {
        ViewManager.Loader.Show("Retrieving Users...");
        var query = VyQuery_GetUsers.Create().IncludeSigningMethods(true);
        var result = await VenlyAPI.Wallet.GetUsers(query);
        ViewManager.Loader.Hide();

        if (result.Success)
        {
            _userList = result.Data.ToList();

            //Store to global
            ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_CachedUsers, result.Data);
        }
        else ViewManager.HandleException(result.Exception);
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_userList, "userList")) return;

        _lstUsers.SetItemSource(_userList);
    }
    #endregion

    #region EVENTS
    private void OnClick_User(VyUserDto user)
    {
        if (IsSelectionMode)
        {
            FinishSelection(user);
            return;
        }

        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_UserDetails, ApiExplorer_UserDetailsVC.DATAKEY_USER, user);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_UserDetails);
    }
    #endregion
}
