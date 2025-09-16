using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Token;

public class ApiExplorer_ViewTokenTypesVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public const string DATAKEY_TOKENTYPES = "tokenTypes";
    public const string DATAKEY_CONTRACT = "sourceContract";

    //DATA
    private List<VyErc1155TokenTypeSummaryDto> _tokenTypeList = null;
    private VyErc1155ContractDto _sourceContract = null;

    //UI
    [UIBind("lst-tokentypes")]private VyControl_TokenTypeListView _lstTokenTypes;

    public ApiExplorer_ViewTokenTypesVC() :
        base(eApiExplorerViewId.TokenApi_ViewErc1155TokenTypes) { }

    #region DATA & UI
    protected override void OnActivate()
    {
        //View Parameters
        ShowNavigateBack = true;
        ShowNavigateHome = false;
        ShowRefresh = false;

        _lstTokenTypes.OnItemSelected += onClick_TokenType;

        if (TryGetBlackboardData(out VyErc1155TokenTypeSummaryDto[] resultArr, localKey:DATAKEY_TOKENTYPES))
        {
            _tokenTypeList = resultArr.ToList();
            NoDataRefresh = true;
        }
        
        if (TryGetBlackboardData(out _sourceContract, localKey: DATAKEY_CONTRACT))
        {
            ShowRefresh = true;
        }
    }

    protected override async Task OnRefreshData()
    {
        if (!ValidateData(_sourceContract, DATAKEY_CONTRACT)) return;

        ViewManager.Loader.Show("Refreshing TokenTypes...");
        var result = await VenlyAPI.Token.GetErc1155TokenTypes(_sourceContract.Chain, _sourceContract.Address);
        ViewManager.Loader.Hide();

        if (result.Success) _tokenTypeList = result.Data.ToList();
        else ViewManager.HandleException(result.Exception);
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_tokenTypeList, DATAKEY_TOKENTYPES)) return;
        //if (!ValidateData(_sourceContract, DATAKEY_CONTRACT)) return;

        _lstTokenTypes.SetItemSource(_tokenTypeList);
    }
    #endregion

    #region EVENTS
    private void onClick_TokenType(VyErc1155TokenTypeSummaryDto tokenTypeSummary)
    {
        if (IsSelectionMode)
        {
            FinishSelection(tokenTypeSummary);
            return;
        }

        //Should come from a Contract Details View
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_Erc1155TokenTypeDetails, ApiExplorer_TokenTypeDetailsVC.DATAKEY_TOKENTYPE_SUMMARY , tokenTypeSummary);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_Erc1155TokenTypeDetails, ApiExplorer_TokenTypeDetailsVC.DATAKEY_CONTRACT , _sourceContract);
        ViewManager.SwitchView(eApiExplorerViewId.TokenApi_Erc1155TokenTypeDetails, ViewId, false);
    }
    #endregion
}
