using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Token;
using Venly.Utils;

[SampleViewMeta(eApiExplorerViewId.TokenApi_Erc1155ContractDetails, "Erc1155 Contract Details")]
public class ApiExplorer_ContractDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public static readonly BlackboardKey<string> KEY_ContractAddr = new BlackboardKey<string>("contract-addr");
    public static readonly BlackboardKey<eVyChain> KEY_ContractChain = new BlackboardKey<eVyChain>("contract-chain");

    //DATA
    private static string _contractAddr;
    private static eVyChain _contractChain;

    private VyErc1155ContractDto _contract;
    private VyErc1155TokenTypeSummaryDto[] _tokenTypes;
    private Texture2D _contractImage;

    public ApiExplorer_ContractDetailsVC() :
        base(eApiExplorerViewId.TokenApi_Erc1155ContractDetails) { }

    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-tokenTypes", onClick_ShowTokenTypes);
        BindButton("btn-archive", onClick_Archive);
        BindButton("btn-create-tokentype", onClick_CreateTokenType);

        //Only show Archive Button when DevMode is active
        ToggleElement("btn-archive", VenlySettings.BackendProvider == eVyBackendProvider.DevMode);
    }

    protected override void OnActivate()
    {
        _contract = null;

        if (!TryGet(KEY_ContractAddr, out _contractAddr))
        {
            ViewManager.Exception.Show($"ContractDetailsVC >> BlackboardData '{KEY_ContractAddr.Name}' not set...");
        }

        if (!TryGet(KEY_ContractChain, out _contractChain))
        {
            ViewManager.Exception.Show($"ContractDetailsVC >> BlackboardData '{KEY_ContractChain.Name}' not set...");
        }

        ViewTitle = $"{_contractChain} Contract Details";
        NoDataRefresh = false;
    }

    protected override async Task OnRefreshData()
    {
        //Retrieve Contract Data
        //----------------------
        using (ViewManager.BeginLoad("Retrieving ERC1155 Contract Info..."))
        {
            var result = await VenlyAPI.Token.GetErc1155Contract(_contractChain, _contractAddr);
            if (!result.Success)
            {
                ViewManager.HandleException(result.Exception);
                return;
            }
            _contract = result.Data;
        }

        //Retrieve Token Types
        //----------------------------
        using (ViewManager.BeginLoad("Retrieving ERC1155 Token Types..."))
        {
            var tokenTypesResult = await VenlyAPI.Token.GetErc1155TokenTypes(_contract.Chain, _contract.Address);
            if (!tokenTypesResult.Success)
            {
                ViewManager.HandleException(tokenTypesResult.Exception);
                return;
            }
            _tokenTypes = tokenTypesResult.Data;
        }

        //Retrieve Image
        //--------------
        using (ViewManager.BeginLoad("Retrieving Contract Image..."))
        {
            var imageResult = await VenlyUnityUtils.DownloadImage(_contract.Metadata.Image);
            _contractImage = imageResult.Success ? imageResult.Data : null;
        }
    }

    protected override void OnRefreshUI()
    {
        //Set Image
        ToggleElement("img-container", _contractImage != null);
        if (_contractImage != null)
            GetElement<VisualElement>("img-contract").style.backgroundImage = new StyleBackground(_contractImage);

        //Refresh UI Elements
        SetLabel("lbl-contract-name", _contract.Metadata.Name);
        SetLabel("lbl-contract-address", _contract.Address);
        SetLabel("lbl-contract-chain", _contract.Chain.GetMemberName());
        SetLabel("lbl-contract-symbol", _contract.Metadata.Symbol);
        SetLabel("lbl-contract-description", _contract.Metadata.Description);
        SetLabel("lbl-contract-externalUrl", _contract.Metadata.ExternalUrl);
        SetLabel("lbl-contract-owner", _contract.Owner);


        //Token Data
        SetLabel("lbl-tokentype-amount", $"{_tokenTypes.Length} Type(s)");
    }

    private
#if ENABLE_VENLY_DEV_MODE
    async
#endif
    void onClick_Archive()
    {
#region DevMode Only (SERVER)
#if ENABLE_VENLY_DEV_MODE
        using (ViewManager.BeginLoad("Archiving Contract..."))
        {
            var result = await VenlyAPI.Token.ArchiveErc1155Contract(_contract.Chain, _contract.Address);

            if (result.Success)
            {
                ViewManager.ClearGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_CachedErc1155Contracts);
                ViewManager.SwitchView(eApiExplorerViewId.TokenApi_ViewErc1155Contracts);
            }
            else ViewManager.HandleException(result.Exception);
        }
#endif
#endregion
    }

    private void onClick_ShowTokenTypes()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_ViewErc1155TokenTypes, ApiExplorer_ViewTokenTypesVC.KEY_Contract, _contract);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_ViewErc1155TokenTypes, ApiExplorer_ViewTokenTypesVC.KEY_TokenTypes, _tokenTypes);
        ViewManager.SwitchView(eApiExplorerViewId.TokenApi_ViewErc1155TokenTypes);
    }

    private void onClick_CreateTokenType()
    {
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_CreateErc1155TokenType, ApiExplorer_CreateErc1155TokenTypeVC.KEY_Contract, _contract);
        ViewManager.SwitchView(eApiExplorerViewId.TokenApi_CreateErc1155TokenType);
    }
}
