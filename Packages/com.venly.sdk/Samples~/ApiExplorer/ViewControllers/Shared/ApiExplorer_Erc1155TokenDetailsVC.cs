using UnityEngine.UIElements;
using Venly.Models.Token;
using Venly.Models.Wallet;
using Venly.Utils;

public class ApiExplorer_Erc1155TokenDetailsVC : SampleViewBase<eApiExplorerViewId>
{

	//DATA
	private VyErc1155TokenDto _token;
	private VyMetadataDto _tokenMetadata;

	//UI
	[UIBind("img-token")] private VisualElement _imgToken;

	[UIBind("fld-animation-urls")] private Foldout _fldAnimationUrls;
	[UIBind("lst-animation-urls")] private VyControl_TypeValueListView _lstAnimationUrls;
	[UIBind("fld-attributes")] private Foldout _fldAttributes;
	[UIBind("lst-attributes")] private VyControl_AttributeListView _lstAttributes;

	public ApiExplorer_Erc1155TokenDetailsVC() : 
		base(eApiExplorerViewId.Shared_Erc1155TokenDetails)
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
		TryGet(ApiExplorer_TokenDetailsDataKeys.KEY_Erc1155Token, out _token);
	}

	protected override void OnRefreshUI()
	{
		if (!ValidateData(_token, "token")) return;
		if (!ValidateData(_tokenMetadata, "token-metadata")) return;

		SetLabel("lbl-token-id", _token.TokenId);
		SetLabel("lbl-token-name", _token.Name);
		SetLabel("lbl-token-url", _token.TokenUri);
		SetLabel("lbl-token-fungible", _token.Fungible??false ? "YES" : "NO");
		SetLabel("lbl-contract-address", _token.ContractAddress);

		BuildAnimationUrls();
		BuildAttributes();

		VenlyUnityUtils.DownloadImage(_token.Image)
			.OnSuccess(tex =>
			{
				_imgToken.style.backgroundImage = new StyleBackground(tex);
			})
			.OnFail(ex => VenlyLog.Exception(ex));
	}

	private void BuildAnimationUrls()
	{
		_fldAttributes.ToggleDisplay(_tokenMetadata.AnimationUrls != null);
		if (_tokenMetadata.AnimationUrls == null) return;

		_fldAnimationUrls.RegisterValueChangedCallback(e =>
		{
			if (e.newValue) _fldAttributes.value = false;
		});

		_fldAnimationUrls.text = $"Animation Urls ({_tokenMetadata.AnimationUrls.Length})";

		_lstAnimationUrls.ToggleDisplay(_tokenMetadata.AnimationUrls.Length > 0);
		if (_tokenMetadata.AnimationUrls.Length == 0) return;
		_lstAnimationUrls.SetItemSource(_tokenMetadata.AnimationUrls);
	}

	private void BuildAttributes()
	{
		_fldAttributes.ToggleDisplay(_tokenMetadata.Attributes != null);
		if (_tokenMetadata.Attributes == null) return;

		_fldAttributes.RegisterValueChangedCallback(e =>
		{
			if (e.newValue) _fldAnimationUrls.value = false;
		});

		_fldAttributes.text = $"Attributes ({_tokenMetadata.Attributes.Length})";

		_lstAnimationUrls.ToggleDisplay(_tokenMetadata.AnimationUrls.Length > 0);
		if (_tokenMetadata.Attributes.Length == 0) return;
		_lstAttributes.SetItemSource(_tokenMetadata.Attributes);
	}
	#endregion

	#region EVENTS
	private void OnClick_Transfer()
	{
		//if (!TryGet(ApiExplorer_TokenDetailsDataKeys.KEY_WALLET, out VyWalletDto wallet)) return;
		//ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransferNft, ApiExplorer_TransferNftVC.KEY_Wallet, wallet);
		//ViewManager.SetViewBlackboardData(eApiExplorerViewId.WalletApi_TransferNft, ApiExplorer_TransferNftVC.KEY_Erc1155Token, _token);

		//ViewManager.SwitchView(eApiExplorerViewId.WalletApi_TransferNft);
	}
	#endregion
}