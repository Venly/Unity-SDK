public class ApiExplorer_LandingDevModeVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_LandingDevModeVC() : 
        base(eApiExplorerViewId.Landing_DevMode) { }

    protected override void OnActivate()
    {
        ShowNavigateBack = false;
        ShowNavigateHome = false;
        ShowRefresh = false;

        BindButton("btn-wallet-api", onClick_WalletAPI);
        BindButton("btn-token-api", onClick_TokenAPI);

#if ENABLE_VENLY_DEV_MODE
        SetLabel("lbl-client-id", VenlySettings.ClientId);
        SetLabel("lbl-backend-provider", VenlySettings.BackendProvider.ToString());
#endif
    }

    private void onClick_WalletAPI()
    {
        ViewManager.SwitchView(eApiExplorerViewId.Main_WalletApi);
    }

    private void onClick_TokenAPI()
    {
        ViewManager.SwitchView(eApiExplorerViewId.Main_TokenApi);
    }

    private void onClick_LoginUser()
    {

    }

    private void onClick_CreateUser()
    {

    }
}
