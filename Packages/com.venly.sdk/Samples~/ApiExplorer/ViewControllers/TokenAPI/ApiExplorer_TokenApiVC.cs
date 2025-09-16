using UnityEngine;

public class ApiExplorer_TokenApiVC : SampleViewBase<eApiExplorerViewId>
{
    public ApiExplorer_TokenApiVC() : 
        base(eApiExplorerViewId.Main_TokenApi) {}

    protected override void OnActivate()
    {
        ShowRefresh = false;

        BindButton_SwitchView("btn-view-contracts", eApiExplorerViewId.TokenApi_ViewErc1155Contracts);
        BindButton_SwitchView("btn-mint-token", eApiExplorerViewId.TokenApi_MintErc1155Token);

#if ENABLE_VENLY_DEV_MODE
        if (!VenlySettings.HasTokenApiAccess)
        {
            if (!HasBlackboardData("realm-access-shown"))
            {
                SetBlackboardData("realm-access-shown", null);
                ViewManager.Info.Show($"The active credentials \'{VenlySettings.ClientId}\' do not have TOKEN API realm access.\n\nSome features of the NFT API samples will not work due to insufficient access (unauthorized).", new Color(0.9f, 0.4f, 0.05f));
            }
        }
#endif
    }
}
