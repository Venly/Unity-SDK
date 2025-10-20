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

[SampleViewMeta(eApiExplorerViewId.TokenApi_CreateErc1155TokenType, "Create Token Type")]
public class ApiExplorer_CreateErc1155TokenTypeVC : SampleViewBase<eApiExplorerViewId>
{
    //UI
    [UIBind("selector-animType")] private DropdownField _selectorAnimUrlType; 
    [UIBind("selector-attrType")] private DropdownField _selectorAttrType;

    //DATA-KEYS
    public static readonly BlackboardKey<VyErc1155ContractDto> KEY_Contract = new BlackboardKey<VyErc1155ContractDto>("contract");
    //public static readonly BlackboardKey<string> KEY_ContractAddr = new BlackboardKey<string>("contract-addr");
    //public static readonly BlackboardKey<eVyChain> KEY_ContractChain = new BlackboardKey<eVyChain>("contract-chain");

    //DATA
    private VyErc1155ContractDto _contract;
    //private string _contractAddr;
    //private eVyChain _contractChain;

    private string _currImageUrl;
    private bool _useAttr;
    private bool _useAnimUrl;

    public ApiExplorer_CreateErc1155TokenTypeVC() :
        base(eApiExplorerViewId.TokenApi_CreateErc1155TokenType) { }

    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-create", onClick_Create);
        BindFocusOut("txt-imageUrl", onFocusOut_ImageUrl);
        BindToggle("toggle-attribute", onToggle_Attribute);
        BindToggle("toggle-animUrl", onToggle_AnimationUrl);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
        NoDataRefresh = true;

        _currImageUrl = "";
        updateImage(null);
        _selectorAnimUrlType.FromEnum(eVyTokenAnimationUrlType.Image);
        _selectorAttrType.FromEnum(eVyTokenAttributeType.Property);

        if (!TryGet(KEY_Contract, out _contract))
        {
            ViewManager.Exception.Show($"CreateTokenTypeVC >> BlackboardData '{KEY_Contract.Name}' not set...");
        }

        //if (!TryGet(KEY_ContractAddr, out _contractAddr))
        //{
        //    ViewManager.Exception.Show($"CreateTokenTypeVC >> BlackboardData '{KEY_ContractAddr.Name}' not set...");
        //}

        //if (!TryGet(KEY_ContractChain, out _contractChain))
        //{
        //    ViewManager.Exception.Show($"CreateTokenTypeVC >> BlackboardData '{KEY_ContractChain.Name}' not set...");
        //}

        onToggle_Attribute(GetToggleValue("toggle-attribute"));
        onToggle_AnimationUrl(GetToggleValue("toggle-animUrl"));
        ToggleElement("toggle-fungible", _contract.Chain == eVyChain.Sui);
    }

    private void updateImage(Texture2D img)
    {
        ToggleElement("img-container", img != null);
        GetElement<VisualElement>("img-contract").style.backgroundImage = img != null ?
            new StyleBackground(img) :
            null;
    }

    private void onToggle_Attribute(bool active)
    {
        _useAttr = active;
        ToggleElement("attr-container", active);
    }

    private void onToggle_AnimationUrl(bool active)
    {
        _useAnimUrl = active;
        ToggleElement("animUrl-container", active);
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
        if (!ValidateData(_contract, "contract")) return;
        if (!ValidateInput("txt-name")) return;
        if (!ValidateInput("txt-description")) return;
        if (!ValidateInput("txt-imageUrl")) return;
        if (!ValidateInput("txt-maxSupply")) return;

        VyErc1155TokenAttributeDto[] attributes = null;
        if (_useAttr)
        {
            if (!ValidateInput("txt-attr-name")) return;
            if (!ValidateInput("txt-attr-value")) return;

            attributes = new VyErc1155TokenAttributeDto[]
            {
                new VyErc1155TokenAttributeDto
                {
                    Type = _selectorAttrType.GetValue<eVyTokenAttributeType>(),
                    Name = GetValue("txt-attr-name"),
                    Value = GetValue("txt-attr-value")
                }
            };
        }

        VyErc1155TokenAnimationUrlDto[] animUrls = null;
        if (_useAnimUrl)
        {
            animUrls = new VyErc1155TokenAnimationUrlDto[]
            {
                new VyErc1155TokenAnimationUrlDto
                {
                    Type = _selectorAnimUrlType.GetValue<eVyTokenAnimationUrlType>(),
                    Value = GetValue("txt-animUrl-value")
                }
            };
        }

        

        var request = new VyCreateErc1155TokenTypeRequest
        {
            Chain = _contract.Chain,
            ContractAddress = _contract.Address,
            Creations = new VyCreateErc1155TokenTypeDto[]
            {
                new VyCreateErc1155TokenTypeDto{
                    Name = GetValue("txt-name"),
                    Description = GetValue("txt-description"),
                    Image = GetValue("txt-imageUrl"),
                    MaxSupply = GetValue<int>("txt-maxSupply"),
                    Burnable = GetToggleValue("toggle-burnable"),
                    Fungible = GetToggleValue("toggle-fungible"),
                    Attributes = attributes,
                    AnimationUrls = animUrls
                }
            }
        };

        //Create Contract
        string creationId;
        using (ViewManager.BeginLoad("Creating..."))
        {
            var response = await VenlyAPI.Token.CreateErc1155TokenType(request);
            if (response.Success) creationId = response.Data.Creations[0].Id;
            else
            {
                ViewManager.HandleException(response.Exception);
                return;
            }
            ;
        }

        //Check Creation
        eVyStatus deployState = eVyStatus.Processing;
        int maxPolls = 20;
        int pollDelay = 3000;
        while (true)
        {
            using (ViewManager.BeginLoad($"Checking Creation... [{maxPolls}]\nStatus = {deployState}"))
            {
                var response = await VenlyAPI.Token.CheckErc1155TokenTypeCreation(creationId);
                if (response.Success)
                {
                    deployState = response.Data.Status ?? eVyStatus.Processing;
                    if(deployState == eVyStatus.Succeeded)
                    {
                        //Go To Contract Details
                        ViewManager.Info.Show("Token Type Creation Succeeded!");
                        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_Erc1155TokenTypeDetails, ApiExplorer_TokenTypeDetailsVC.KEY_Contract, _contract);
                        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_Erc1155TokenTypeDetails, ApiExplorer_TokenTypeDetailsVC.KEY_TokenTypeId, response.Data.TokenTypeId.Value);
                        ViewManager.SwitchView(eApiExplorerViewId.TokenApi_Erc1155TokenTypeDetails, CurrentBackTarget);
                        break;
                    }
                    else if(deployState == eVyStatus.Failed)
                    {
                        ViewManager.Exception.Show($"Token Type Creation Failed...\n(id={creationId})");
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
