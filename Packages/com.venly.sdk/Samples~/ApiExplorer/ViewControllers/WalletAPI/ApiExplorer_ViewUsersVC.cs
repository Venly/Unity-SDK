using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venly;
using Venly.Models.Wallet;

[SampleViewMeta(eApiExplorerViewId.WalletApi_ViewUsers, "View Users")]
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

        //Check for Cached Users
        if (ViewManager.TryGetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_CachedUsers, out var cachedUsers))
        {
            _userList = cachedUsers;
            NoDataRefresh = true;
        }
    }

    protected override async Task OnRefreshData()
    {
        using (ViewManager.BeginLoad("Retrieving Users..."))
        {
            var query = VyQuery_GetUsers.Create().IncludeSigningMethods(true);
            var result = await VenlyAPI.Wallet.GetUsers(query);

            if (result.Success)
            {
                _userList = result.Data.ToList();

                //Store to global
                ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_CachedUsers, _userList);
            }
            else ViewManager.HandleException(result.Exception);
        }
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

        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_UserDetails, ApiExplorer_UserDetailsVC.KEY_User, user);
        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_UserDetails);
    }
    #endregion
}
