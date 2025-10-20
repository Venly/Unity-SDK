using UnityEngine.UIElements;
using Venly;
using Venly.Models.Wallet;

[SampleViewMeta(eApiExplorerViewId.WalletApi_CreateUser, "Create User")]
public class ApiExplorer_CreateUser : SampleViewBase<eApiExplorerViewId>
{

    public ApiExplorer_CreateUser() : 
        base(eApiExplorerViewId.WalletApi_CreateUser) { }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-create", OnClick_CreateUser);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
    }
    #endregion

    #region EVENTS
    private void OnClick_CreateUser()
    {
        var createParams = new VyCreateUserRequest()
        {
            Reference = GetValue("txt-reference"),
            SigningMethod = new VyCreatePinSigningMethodRequest
            {
                Value = GetValue("txt-pincode")
            }
        };

        ViewManager.Loader.Show("Creating User..");
        VenlyAPI.Wallet.CreateUser(createParams)
            .OnSuccess(user =>
            {
                ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_UserDetails, ApiExplorer_UserDetailsVC.KEY_User, user);
                ViewManager.SwitchView(eApiExplorerViewId.WalletApi_UserDetails, CurrentBackTarget);
            })
            .OnFail(ViewManager.Exception.Show)
            .Finally(ViewManager.Loader.Hide);
    }
    #endregion
}
