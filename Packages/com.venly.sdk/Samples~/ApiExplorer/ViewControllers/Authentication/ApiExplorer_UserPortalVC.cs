using System;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Wallet;
using Venly.Utils;

public class ApiExplorer_UserPortalVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_PROVIDER_USER_ID = "user-id";
    public const string DATAKEY_PINCODE = "pincode";

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
        //BindButton("btn-market", OnClick_Market);
    }

    protected override void OnActivate()
    {
        ShowNavigateBack = false;
    }

    protected override async Task OnRefreshData()
    {
        try
        {
            //Check if User exists
            ViewManager.Loader.Show("Checking for User...");
            _hasUser = await VenlyAPI.ProviderExtensions.HasUser().AwaitResult();

            if (_hasUser)
            {
                ViewManager.Loader.Show("Retrieving User...");
                _user = await VenlyAPI.ProviderExtensions.GetUser().AwaitResult();
            }
            else
            {
                ViewManager.Loader.Show("Creating User...");
                if (TryGetBlackboardData(out string pincode, localKey: DATAKEY_PINCODE))
                {
                    TryGetBlackboardDataRaw(out object providerUserId, localKey: DATAKEY_PROVIDER_USER_ID);

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

                    _user = await VenlyAPI.ProviderExtensions.CreateUser(request).AwaitResult();
                    _hasUser = true;
                }
            }

            ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_User, _user);

            //Check if Wallet is present
            ViewManager.Loader.Show("Checking for Wallet...");
            _hasWallet = await VenlyAPI.ProviderExtensions.HasWallet().AwaitResult();
            if (_hasWallet)
            {
                ViewManager.Loader.Show("Retrieving Wallet...");
                _wallet = await VenlyAPI.ProviderExtensions.GetWallet().AwaitResult();
            }
            else
            {
                ViewManager.Loader.Show("Creating Wallet...");
                TryGetBlackboardDataRaw(out object providerUserId, localKey: DATAKEY_PROVIDER_USER_ID);

                //Create Wallet Params
                var request = new VyCreateWalletRequest
                {
                    Chain = eVyChain.Matic,
                    Description = $"Wallet created by Unity API Explorer. (provider={VenlySettings.BackendProvider.GetMemberName()}, id={providerUserId?.ToString()})",
                    Identifier = $"api-explorer-created ({VenlySettings.BackendProvider.GetMemberName()})",
                    UserId = _user.Id
                };

                _wallet = await VenlyAPI.ProviderExtensions.CreateWallet(request).AwaitResult();
                _hasWallet = true;
            }
        }
        catch (Exception ex)
        {
            ViewManager.HandleException(ex);
        }
        finally
        {
            ViewManager.Loader.Hide();
        }
    }

    protected override void OnRefreshUI()
    {
        base.OnRefreshUI();

        SetLabel("btn-wallet", _hasWallet?"Show Wallet":"Create Wallet");
        SetLabel("btn-market", _hasUser?"Show User":"Create User");
    }

    private void OnClick_Wallet()
    {
        if (_hasWallet)
        {
            ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_WalletDetails, ApiExplorer_WalletDetailsVC.DATAKEY_WALLET, _wallet);
            ViewManager.SwitchView(eApiExplorerViewId.WalletApi_WalletDetails, this, false);
        }
    }

    //private async void OnClick_Market()
    //{
    //    try
    //    {
    //        if (!_hasMarketUser)
    //        {
    //            ViewManager.Loader.Show("Creating Market User...");
    //            TryGetBlackboardDataRaw(out object providerUserId, localKey: DATAKEY_USER_ID);
    //            if (providerUserId == null) providerUserId = "generic";

    //            var request = new VyCreateSubUserRequest
    //            {
    //                Nickname = $"{VenlySettings.BackendProvider.GetMemberName()}_{providerUserId}",
    //                Type = eVyUserType.SubUser
    //            };

    //            _marketUser = await VenlyAPI.ProviderExtensions.CreateMarketUserForUser(request).AwaitResult();
    //            _hasMarketUser = true;
    //            ViewManager.Loader.Hide();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        ViewManager.Loader.Hide();
    //        ViewManager.HandleException(ex);
    //        return;
    //    }

    //    ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_UserMarketProfile, _marketUser);
    //    ViewManager.SwitchView(eApiExplorerViewId.MarketApi_SubUserDetails, this, false);
    //}
}
