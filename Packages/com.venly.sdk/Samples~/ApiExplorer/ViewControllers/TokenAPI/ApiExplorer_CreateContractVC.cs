using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Token;
using Venly.Models.Wallet;
using Venly.Utils;

[SampleViewMeta(eApiExplorerViewId.TokenApi_CreateErc1155Contract, "Create ERC1155 Contract")]
public class ApiExplorer_CreateErc1155ContractVC : SampleViewBase<eApiExplorerViewId>
{
    [UIBind("selector-chain")] private DropdownField _selectorChains;
    private string _currImageUrl;

    public ApiExplorer_CreateErc1155ContractVC() :
        base(eApiExplorerViewId.TokenApi_CreateErc1155Contract) { }

    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-create", onClick_Create);
        BindButton("btn-select-owner", OnClick_SelectOwner);
        BindFocusOut("txt-imageUrl", onFocusOut_ImageUrl);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
        NoDataRefresh = true;

        //Populate Selector
        _selectorChains.FromEnum(eVyChain.Sui);
        _selectorChains.OnEnumChanged<eVyChain>(newVal =>
        {
            updateTitle();
        });

        _currImageUrl = "";
        updateImage(null);
        updateTitle();
    }

    private void updateTitle()
    {
        SetViewTitle($"Create {_selectorChains.text} Contract");
    }

    private void updateImage(Texture2D img)
    {
        ToggleElement("img-container", img != null);
        GetElement<VisualElement>("img-contract").style.backgroundImage = img != null ?
            new StyleBackground(img) :
            null;
    }

    private void OnClick_SelectOwner()
    {
        ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewWallets, "Select Wallet")
            .OnComplete(result =>
            {
                if (result.Success)
                {
                    var wallet = result.Data as VyWalletDto;
                    SetLabel("txt-owner", wallet.Address);
                }
            });
    }

    private void onFocusOut_ImageUrl()
    {
        var newValue = GetValue("txt-imageUrl");
        if (newValue == _currImageUrl) return;

        _currImageUrl = newValue;

        if (string.IsNullOrEmpty(_currImageUrl))
        {
            updateImage(null);
            return;
        }

        VenlyUnityUtils.DownloadImage(_currImageUrl)
            .OnSuccess(updateImage)
            .OnFail(ex => {
                SetLabel("txt-imageUrl", "");
                onFocusOut_ImageUrl();
                ViewManager.HandleException(ex);
            });
    }

    private async void onClick_Create()
    {
        if (!ValidateInput("txt-name")) return;
        if (!ValidateInput("txt-symbol")) return;
        if (!ValidateInput("txt-description")) return;
        if (!ValidateInput("txt-imageUrl")) return;
        if (!ValidateInput("txt-externalUrl")) return;
        if (!ValidateInput("txt-owner")) return;

        var request = new VyCreateErc1155ContractRequest
        {
            Chain = _selectorChains.GetValue<eVyChain>(),
            Name = GetValue("txt-name"),
            Symbol = GetValue("txt-symbol"),
            Description = GetValue("txt-description"),
            Image = GetValue("txt-imageUrl"),
            ExternalUrl = GetValue("txt-externalUrl"),
            Owner = GetValue("txt-owner")
        };

        //Create Contract
        string deploymentId;
        using (ViewManager.BeginLoad("Deploying..."))
        {
            var response = await VenlyAPI.Token.CreateErc1155Contract(request);
            if (response.Success) deploymentId = response.Data.Id;
            else
            {
                ViewManager.HandleException(response.Exception);
                return;
            }
            ;
        }

        //Check Deployment
        eVyStatus deployState = eVyStatus.Processing;
        int maxPolls = 20;
        int pollDelay = 3000;
        while (true)
        {
            using (ViewManager.BeginLoad($"Checking Deployment... [{maxPolls}]\nStatus = {deployState}"))
            {
                var response = await VenlyAPI.Token.CheckErc1155ContractDeployment(deploymentId);
                if (response.Success)
                {
                    deployState = response.Data.Status ?? eVyStatus.Processing;
                    if(deployState == eVyStatus.Succeeded)
                    {
                        //Go To Contract Details
                        ViewManager.Info.Show("Contract Creation Succeeded!");
                        ViewManager.ClearGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_CachedErc1155Contracts);
                        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_Erc1155ContractDetails, ApiExplorer_ContractDetailsVC.KEY_ContractAddr, response.Data.Address);
                        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_Erc1155ContractDetails, ApiExplorer_ContractDetailsVC.KEY_ContractChain, response.Data.Chain.Value);
                        ViewManager.SwitchView(eApiExplorerViewId.TokenApi_Erc1155ContractDetails, CurrentBackTarget);
                        break;
                    }
                    else if(deployState == eVyStatus.Failed)
                    {
                        ViewManager.Exception.Show($"Contract Creation Failed...\n(id={deploymentId})");
                        break;
                    }
                }
                else
                {
                    ViewManager.HandleException(response.Exception);
                    break;
                }

                --maxPolls;
                if (maxPolls < 0) break;

                //Wait for next poll
                await Task.Delay(pollDelay);
            }
        }
    }
}
