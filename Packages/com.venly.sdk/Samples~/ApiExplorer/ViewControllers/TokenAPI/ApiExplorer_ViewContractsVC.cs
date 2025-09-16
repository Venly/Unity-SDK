using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Token;

public class ApiExplorer_ViewContractsVC : SampleViewBase<eApiExplorerViewId>
{
    //UI
    [UIBind("lst-contracts")] private VyControl_ContractListView _lstContracts;
    private List<VyErc1155ContractDto> _contractList = null;

    public ApiExplorer_ViewContractsVC() :
        base(eApiExplorerViewId.TokenApi_ViewErc1155Contracts) { }

    #region DATA & UI
    protected override void OnActivate()
    {
        _lstContracts.OnItemSelected += onClick_Contract;

        //Check for Cached Wallets
        if (TryGetBlackboardData(out VyErc1155ContractDto[] resultArr, globalKey: ApiExplorer_GlobalKeys.DATA_CachedErc1155Contracts))
        {
            _contractList = resultArr.ToList();
            NoDataRefresh = true;
        }
    }

    protected override async Task OnRefreshData()
    {
        ViewManager.Loader.Show("Retrieving Contracts...");
        var result = await VenlyAPI.Token.GetErc1155Contracts();
        ViewManager.Loader.Hide();

        if (result.Success)
        {
            _contractList = result.Data.ToList();
            _contractList.RemoveAll(c => c.OnChainStatus != eVyStatus.Succeeded);

            //Store to cache
            ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_CachedErc1155Contracts, result.Data);
        }
        else ViewManager.HandleException(result.Exception);
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_contractList, "contractList")) return;
        _lstContracts.SetItemSource(_contractList);
    }
    #endregion

    #region EVENTS
    private void onClick_Contract(VyErc1155ContractDto contract)
    {
        if (IsSelectionMode)
        {
            FinishSelection(contract);
            return;
        }

        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_Erc1155ContractDetails, ApiExplorer_ContractDetailsVC.DATAKEY_CONTRACT , contract);
        ViewManager.SwitchView(eApiExplorerViewId.TokenApi_Erc1155ContractDetails, ViewId, false);
    }
    #endregion
}
