using System;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Wallet;
using Venly.Utils;

public class ApiExplorer_UserPortalVC : SampleViewBase<eApiExplorerViewId>
{
    public eVyChain DefaultChain = eVyChain.Sui;

    //DATA-KEYS (typed)
    public static readonly BlackboardKey<string> KEY_PROVIDER_USER_ID = new BlackboardKey<string>("user-id");
    public static readonly BlackboardKey<string> KEY_PINCODE = new BlackboardKey<string>("pincode");

    //DATA
    private bool _hasWallet = false;
    private VyWalletDto _wallet;

    private bool _hasUser = false;
    private VyUserDto _user;

    public ApiExplorer_UserPortalVC() :
        base(eApiExplorerViewId.Auth_UserPortal)
    { }

    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-wallet", OnClick_Wallet);
        BindButton("btn-user", OnClick_User);
    }

    protected override void OnActivate()
    {
        ShowNavigateBack = false;
        SetLabel("lbl-backend-provider", VenlySettings.BackendProvider.ToString());
    }

    protected override async Task OnRefreshData()
    {
        try
        {
            //Check if User exists
            using (ViewManager.BeginLoad("Checking for User..."))
            {
                _hasUser = await VenlyAPI.ProviderExtensions.HasUser().AwaitResult();
            }

            if (_hasUser)
            {
                using (ViewManager.BeginLoad("Retrieving User..."))
                {
                    _user = await VenlyAPI.ProviderExtensions.GetUser().AwaitResult();
                }
            }
            else
            {
                if (TryGet(KEY_PINCODE, out var pincode))
                {
                    TryGetRaw(KEY_PROVIDER_USER_ID, out object providerUserId);

                    //Create User Params
                    var request = new VyCreateUserRequest
                    {
                        Reference =
                            $"User created by Unity API Explorer. (provider={VenlySettings.BackendProvider.GetMemberName()}, id={providerUserId?.ToString()})",
                        SigningMethod = new VyCreatePinSigningMethodRequest
                        {
                            Value = pincode
                        }
                    };

                    using (ViewManager.BeginLoad("Creating User..."))
                    {
                        _user = await VenlyAPI.ProviderExtensions.CreateUser(request).AwaitResult();
                        _hasUser = true;
                    }
                }
            }

            ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_User, _user);

            //Check if Wallet is present
            using (ViewManager.BeginLoad("Checking for Wallet..."))
            {
                _hasWallet = await VenlyAPI.ProviderExtensions.HasWallet().AwaitResult();
            }
            if (_hasWallet)
            {
                using (ViewManager.BeginLoad("Retrieving Wallet..."))
                {
                    _wallet = await VenlyAPI.ProviderExtensions.GetWallet().AwaitResult();
                }
            }
            else
            {
                TryGetRaw(KEY_PROVIDER_USER_ID, out object providerUserId);

                //Create Wallet Params
                var request = new VyCreateWalletRequest
                {
                    Chain = DefaultChain,
                    Description = $"Wallet created by Unity API Explorer. (provider={VenlySettings.BackendProvider.GetMemberName()}, id={providerUserId})",
                    Identifier = $"api-explorer-created ({VenlySettings.BackendProvider.GetMemberName()})",
                    UserId = _user.Id
                };

                using (ViewManager.BeginLoad("Creating Wallet..."))
                {
                    _wallet = await VenlyAPI.ProviderExtensions.CreateWallet(request).AwaitResult();
                    _hasWallet = true;
                }
            }
        }
        catch (Exception ex)
        {
            ViewManager.HandleException(ex);
        }
    }

    protected override void OnRefreshUI()
    {
        base.OnRefreshUI();

        SetLabel("btn-wallet", _hasWallet?"Show Wallet":"Create Wallet");
        SetLabel("btn-user", _hasUser?"Show User":"Create User");
    }

    private void OnClick_Wallet()
    {
        if (_hasWallet)
        {
            ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_WalletDetails, ApiExplorer_WalletDetailsVC.KEY_Wallet, _wallet);
            ViewManager.SwitchView(eApiExplorerViewId.WalletApi_WalletDetails, this, false);
        }
    }

    private void OnClick_User()
    {
        if (_hasWallet)
        {
            ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_UserDetails, ApiExplorer_UserDetailsVC.KEY_User, _user);
            ViewManager.SwitchView(eApiExplorerViewId.WalletApi_UserDetails, this, false);
        }
    }
}
