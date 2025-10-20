using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Venly.Models.Token;
using Venly.Models.Wallet;
using Venly.Utils;

[SampleViewMeta(eApiExplorerViewId.WalletApi_ViewNftDetails, "NFT Details")]
public class ApiExplorer_ViewNftDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA
    private VyNftDto _nft;

    //UI
    [UIBind("img-token")] private VisualElement _imgToken;

    [UIBind("fld-animation-urls")] private Foldout _fldAnimationUrls;
    [UIBind("lst-animation-urls")] private VyControl_TypeValueListView _lstAnimationUrls;
    [UIBind("fld-attributes")] private Foldout _fldAttributes;
    [UIBind("lst-attributes")] private VyControl_AttributeListView _lstAttributes;

    public ApiExplorer_ViewNftDetailsVC() : 
        base(eApiExplorerViewId.WalletApi_ViewNftDetails)
    {
    }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        //Make sure UIBind Elements are configured
        base.OnBindElements(root);

        BindButton("btn-transfer", OnClick_Transfer);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
        ShowNavigateHome = false;

        _fldAnimationUrls.value = false;
        _fldAttributes.value = false;

        //Retrieve Token from Blackboard (should be set by calling view)
        TryGet(ApiExplorer_TokenDetailsDataKeys.KEY_Nft, out _nft);
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_nft, "token")) return;
        //if (!ValidateData(_tokenMetadata, "token-metadata")) return;

        SetLabel("lbl-token-id", _nft.Id);
        SetLabel("lbl-contract-name", _nft.Contract.Name);
        //SetLabel("lbl-token-url", _nft.Url);
        SetLabel("lbl-token-fungible", _nft.Fungible??false ? "YES" : "NO");
        SetLabel("lbl-contract-address", _nft.Contract.Address);

        BuildAnimationUrls();
        BuildAttributes();

        VenlyUnityUtils.DownloadImage(_nft.ImageUrl)
            .OnSuccess(tex =>
            {
                _imgToken.style.backgroundImage = new StyleBackground(tex);
            })
            .OnFail(ex => VenlyLog.Exception(ex));
    }

    private void BuildAnimationUrls()
    {
        _fldAttributes.ToggleDisplay(_nft.AnimationUrls != null);
        if (_nft.AnimationUrls == null) return;

        _fldAnimationUrls.RegisterValueChangedCallback(e =>
        {
            if (e.newValue) _fldAttributes.value = false;
        });

        _fldAnimationUrls.text = $"Animation Urls ({_nft.AnimationUrls.Length})";

        _lstAnimationUrls.ToggleDisplay(_nft.AnimationUrls.Length > 0);
        if (_nft.AnimationUrls.Length == 0) return;
        _lstAnimationUrls.SetItemSource(_nft.AnimationUrls);
    }

    private void BuildAttributes()
    {
        _fldAttributes.ToggleDisplay(_nft.Attributes != null);
        if (_nft.Attributes == null) return;

        _fldAttributes.RegisterValueChangedCallback(e =>
        {
            if (e.newValue) _fldAnimationUrls.value = false;
        });

        _fldAttributes.text = $"Attributes ({_nft.Attributes.Length})";

        _lstAttributes.ToggleDisplay(_nft.Attributes.Length > 0);
        if (_nft.Attributes.Length == 0) return;
        _lstAttributes.SetItemSource(_nft.Attributes);
    }
    #endregion

    #region EVENTS
    private void OnClick_Transfer()
    {
        if (!TryGet(ApiExplorer_TokenDetailsDataKeys.KEY_WALLET, out VyWalletDto wallet)) return;
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransferNft, ApiExplorer_TransferNftVC.KEY_Wallet, wallet);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransferNft, ApiExplorer_TransferNftVC.KEY_Token, _nft);

        ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransferNft);
    }
    #endregion
}