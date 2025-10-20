using System;
using UnityEngine.UIElements;
using Venly;
using Venly.Core;
using Venly.Models.Shared;
using Venly.Models.Wallet;
using static Venly.VenlyAPI;

public class ApiExplorer_CreateWalletVC : SampleViewBase<eApiExplorerViewId>
{
    [UIBind("selector-chain")] private DropdownField _selectorChains;

    public static readonly BlackboardKey<VyUserDto> KEY_User = new BlackboardKey<VyUserDto>("user");

    private VyUserDto _user;
    private bool _hasExistingUser => _user != null;

    public ApiExplorer_CreateWalletVC() : 
        base(eApiExplorerViewId.WalletApi_CreateWallet) { }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-create", OnClick_CreateWallet);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;

        //Populate Selector
        _selectorChains.FromEnum(eVyChain.Sui);

        //Get User
        TryGet(KEY_User, out _user);
    }

    #endregion

    #region EVENTS
    private async void OnClick_CreateWallet()
    {
        if (!ValidateInput("txt-pincode", "pincode")) return;

        //Create User if non-existing
        if (!_hasExistingUser)
        {
            var createUser = new VyCreateUserRequest
            {
                Reference = "auto-created by API Explorer",
                SigningMethod = new VyCreatePinSigningMethodRequest
                {
                    Value = GetValue("txt-pincode")
                }
            };

            using (ViewManager.BeginLoad("Creating User.."))
            {
                var userResult = await VenlyAPI.Wallet.CreateUser(createUser);
                if (!userResult.Success)
                {
                    ViewManager.HandleException(userResult.Exception);
                    return;
                }
                _user = userResult.Data;
            }
        }

        //Get UserAuth
        if (!ValidateData(_user, "user")) return;
        if (!_user.TryGetPinAuth(GetValue("txt-pincode"), out var userAuth))
        {
            ViewManager.HandleException(new ArgumentException("Failed to retrieve a valid UserAuth..."));
            return;
        }

        //Create Wallet
        var createParams = new VyCreateWalletRequest()
        {
            Chain = _selectorChains.GetValue<eVyChain>(),
            Description = GetValue("txt-description"),
            Identifier = GetValue("txt-identifier"),
            UserId = _user.Id
        };

        using (ViewManager.BeginLoad("Creating Wallet.."))
        {
            var walletResult = await VenlyAPI.Wallet.CreateWallet(createParams, userAuth);
            if (!walletResult.Success)
            {
                ViewManager.HandleException(walletResult.Exception);
                return;
            }

            ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_WalletDetails, ApiExplorer_WalletDetailsVC.KEY_Wallet, walletResult.Data);
            ViewManager.SwitchView(eApiExplorerViewId.WalletApi_WalletDetails, CurrentBackTarget);
        }
    }
    #endregion
}
