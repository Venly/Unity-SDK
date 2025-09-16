using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Token;
using Venly.Utils;

public class ApiExplorer_TokenTypeDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_CONTRACT = "contract";
    public const string DATAKEY_TOKENTYPE = "tokentype";
    public const string DATAKEY_TOKENTYPE_SUMMARY = "tokentype-summary";

    //DATA
    private VyErc1155TokenTypeSummaryDto _tokenTypeSummary;
    private VyErc1155TokenTypeDto _tokenType;
    private VyErc1155ContractDto _sourceContract;

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
        TryGetBlackboardData(out _tokenTypeSummary, localKey: DATAKEY_TOKENTYPE_SUMMARY);
        TryGetBlackboardData(out _sourceContract, localKey: DATAKEY_CONTRACT);
    }

    protected override async Task OnRefreshData()
    {
        ViewManager.Loader.Show("Refreshing TokenType...");
        var result = await VenlyAPI.Token.GetErc1155TokenType(_sourceContract.Chain,_sourceContract.Address, _tokenTypeSummary.TokenTypeId);
        ViewManager.Loader.Hide();

        if(result.Success) _tokenType = result.Data;
        else ViewManager.HandleException(result.Exception);
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_sourceContract, DATAKEY_CONTRACT)) return;
        if (!ValidateData(_tokenType, DATAKEY_TOKENTYPE)) return;

        //Set Data
        SetLabel("lbl-token-id", _tokenType.TokenTypeId.ToString());
        SetLabel("lbl-token-name", _tokenType.Metadata.Name);
        SetLabel("lbl-token-description", _tokenType.Metadata.Description);
        SetLabel("lbl-token-supply", $"{_tokenType.Supply.Current} / {_tokenType.Supply.Max}");
        SetLabel("lbl-token-chain", _sourceContract.Chain.GetMemberName());
        SetLabel("lbl-token-externalUrl", _tokenType.Metadata.ExternalUrl);
        SetLabel("lbl-token-fungible", _tokenType.Metadata.Fungible??false ? "YES" : "NO");
        SetLabel("lbl-contract-name", $"{_sourceContract.Metadata.Name}");

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
        VenlyUnityUtils.DownloadImage(_tokenType.Metadata.Image)
            .OnComplete(result =>
            {
                ToggleElement("img-container", result.Success);
                if (result.Success)_imgToken.style.backgroundImage = new StyleBackground(result.Data);
                else VenlyLog.Exception(result.Exception);
            });
    }
    #endregion

    #region EVENTS
    private void onClick_Mint()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_MintErc1155Token, ApiExplorer_MintTokenVC.DATAKEY_CONTRACT, _sourceContract);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_MintErc1155Token, ApiExplorer_MintTokenVC.DATAKEY_TOKENTYPE_SUMMARY, _tokenTypeSummary);
        ViewManager.SwitchView(eApiExplorerViewId.TokenApi_MintErc1155Token, ViewId, false);
    }
    #endregion
}