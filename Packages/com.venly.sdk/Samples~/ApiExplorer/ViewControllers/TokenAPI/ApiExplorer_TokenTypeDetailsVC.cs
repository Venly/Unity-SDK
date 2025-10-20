using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Token;
using Venly.Utils;

[SampleViewMeta(eApiExplorerViewId.TokenApi_Erc1155TokenTypeDetails, "Erc1155 Token Type Details")]
public class ApiExplorer_TokenTypeDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public static readonly BlackboardKey<VyErc1155ContractDto> KEY_Contract = new BlackboardKey<VyErc1155ContractDto>("contract");
    //public static readonly BlackboardKey<eVyChain> KEY_ContractChain = new BlackboardKey<eVyChain>("contract-chain");
    public static readonly BlackboardKey<int> KEY_TokenTypeId = new BlackboardKey<int>("tokentype-id");

    //DATA
    private VyErc1155ContractDto _contract;
    //private eVyChain _contractChain;
    private int _tokenTypeId;

    private VyErc1155TokenTypeDto _tokenType;

    //UI
    [UIBind("img-tokentype")] private VisualElement _imgToken;
    [UIBind("fld-animation-urls")] private Foldout _fldAnimationUrls;
    [UIBind("lst-animation-urls")] private VyControl_TypeValueListView _lstAnimationUrls;
    [UIBind("fld-attributes")] private Foldout _fldAttributes;
    [UIBind("lst-attributes")] private VyControl_AttributeListView _lstAttributes;


    public ApiExplorer_TokenTypeDetailsVC() :
        base(eApiExplorerViewId.TokenApi_Erc1155TokenTypeDetails)
    {
    }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-mint", onClick_Mint);
    }

    protected override void OnActivate()
    {
        _fldAnimationUrls.value = false;
        _fldAttributes.value = false;

        //Retrieve Token from Blackboard (should be set by calling view)
        if (!TryGet(KEY_Contract, out _contract))
        {
            ViewManager.Exception.Show($"TokenTypeDetailsVC >> BlackboardData '{KEY_Contract.Name}' not set...");
        }

        //if (!TryGet(KEY_ContractChain, out _contractChain))
        //{
        //    ViewManager.Exception.Show($"TokenTypeDetailsVC >> BlackboardData '{KEY_ContractChain.Name}' not set...");
        //}

        if (!TryGet(KEY_TokenTypeId, out _tokenTypeId))
        {
            ViewManager.Exception.Show($"TokenTypeDetailsVC >> BlackboardData '{KEY_TokenTypeId.Name}' not set...");
        }
    }

    protected override async Task OnRefreshData()
    {
        using (ViewManager.BeginLoad("Refreshing TokenType..."))
        {
            var result = await VenlyAPI.Token.GetErc1155TokenType(_contract.Chain, _contract.Address, _tokenTypeId);
            if(result.Success) _tokenType = result.Data;
            else ViewManager.HandleException(result.Exception);
        }
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_contract, "contract")) return;
        if (!ValidateData(_tokenType, "tokentype")) return;

        //Set Data
        SetLabel("lbl-token-id", _tokenType.TokenTypeId.ToString());
        SetLabel("lbl-token-name", _tokenType.Metadata.Name);
        SetLabel("lbl-token-description", _tokenType.Metadata.Description);
        SetLabel("lbl-token-supply", $"{_tokenType.Supply.Current} / {_tokenType.Supply.Max}");
        SetLabel("lbl-token-chain", _contract.Chain.GetMemberName());
        SetLabel("lbl-token-externalUrl", _tokenType.Metadata.ExternalUrl);
        SetLabel("lbl-token-fungible", _tokenType.Metadata.Fungible??false ? "YES" : "NO");
        SetLabel("lbl-contract-name", $"{_contract.Metadata.Name}");

        _lstAnimationUrls.SetItemSource(_tokenType.Metadata.AnimationUrls);
        _lstAnimationUrls.ToggleDisplay(_tokenType.Metadata.AnimationUrls.Length > 0);

        _lstAttributes.SetItemSource(_tokenType.Metadata.Attributes);
        _lstAttributes.ToggleDisplay(_tokenType.Metadata.Attributes.Length > 0);

        //Manage FoldOuts
        _fldAttributes.value = false;
        _fldAttributes.text = $"Attributes ({_tokenType.Metadata.Attributes.Length})";
        _fldAttributes.RegisterValueChangedCallback(e =>
        {
            if (e.newValue) _fldAnimationUrls.value = false;
        });

        _fldAnimationUrls.value = false;
        _fldAnimationUrls.text = $"Animation Urls ({_tokenType.Metadata.AnimationUrls.Length})";
        _fldAnimationUrls.RegisterValueChangedCallback(e =>
        {
            if (e.newValue) _fldAttributes.value = false;
        });

        //Image
        if (!string.IsNullOrEmpty(_tokenType.Metadata.Image))
        {
            VenlyUnityUtils.DownloadImage(_tokenType.Metadata.Image)
                .OnComplete(result =>
                {
                    ToggleElement("img-container", result.Success);
                    if (result.Success) _imgToken.style.backgroundImage = new StyleBackground(result.Data);
                    else VenlyLog.Exception(result.Exception);
                });
        }
    }
    #endregion

    #region EVENTS
    private void onClick_Mint()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_MintErc1155Token, ApiExplorer_MintTokenVC.KEY_Contract, _contract);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_MintErc1155Token, ApiExplorer_MintTokenVC.KEY_TokenType, _tokenType);
        ViewManager.SwitchView(eApiExplorerViewId.TokenApi_MintErc1155Token, ViewId, false);
    }
    #endregion
}