using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Token;

[SampleViewMeta(eApiExplorerViewId.TokenApi_ViewErc1155TokenTypes, "View Erc1155 Token Types")]
public class ApiExplorer_ViewTokenTypesVC : SampleViewBase<eApiExplorerViewId>
{
	//DATA-KEYS
	public static readonly BlackboardKey<VyErc1155TokenTypeSummaryDto[]> KEY_TokenTypes = new BlackboardKey<VyErc1155TokenTypeSummaryDto[]>("tokenTypes");
	public static readonly BlackboardKey<VyErc1155ContractDto> KEY_Contract = new BlackboardKey<VyErc1155ContractDto>("sourceContract");

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

		if (TryGet(KEY_TokenTypes, out VyErc1155TokenTypeSummaryDto[] resultArr))
		{
			_tokenTypeList = resultArr.ToList();
			NoDataRefresh = true;
		}
		
		if (TryGet(KEY_Contract, out _sourceContract))
		{
			ShowRefresh = true;
		}
	}

	protected override async Task OnRefreshData()
	{
		if (!ValidateData(_sourceContract, "contract")) return;

		using (ViewManager.BeginLoad("Refreshing TokenTypes..."))
		{
			var result = await VenlyAPI.Token.GetErc1155TokenTypes(_sourceContract.Chain, _sourceContract.Address);
			if (result.Success) _tokenTypeList = result.Data.ToList();
			else ViewManager.HandleException(result.Exception);
		}
	}

	protected override void OnRefreshUI()
	{
		if (!ValidateData(_tokenTypeList, "tokenTypes")) return;
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
		ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_Erc1155TokenTypeDetails, ApiExplorer_TokenTypeDetailsVC.KEY_TokenTypeId , tokenTypeSummary.TokenTypeId);
		ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_Erc1155TokenTypeDetails, ApiExplorer_TokenTypeDetailsVC.KEY_Contract , _sourceContract);
		ViewManager.SwitchView(eApiExplorerViewId.TokenApi_Erc1155TokenTypeDetails, ViewId, false);
	}
	#endregion
}
