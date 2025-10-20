using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Token;

[SampleViewMeta(eApiExplorerViewId.TokenApi_ViewErc1155Contracts, "View Erc1155 Contracts")]
public class ApiExplorer_ViewContractsVC : SampleViewBase<eApiExplorerViewId>
{
    //UI
    [UIBind("lst-contracts")] private VyControl_ContractListView _lstContracts;
    private List<VyErc1155ContractSummaryDto> _contractList = null;

    public ApiExplorer_ViewContractsVC() :
        base(eApiExplorerViewId.TokenApi_ViewErc1155Contracts) { }

    #region DATA & UI
    protected override void OnActivate()
    {
        _lstContracts.OnItemSelected += onClick_Contract;

        //Check for Cached Contracts
        if (ViewManager.TryGetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_CachedErc1155Contracts, out var cachedContracts))
        {
            _contractList = cachedContracts;
            NoDataRefresh = true;
        }
    }

    protected override void OnDeactivate()
    {
        if (_lstContracts != null) _lstContracts.OnItemSelected -= onClick_Contract;
    }

    protected override async Task OnRefreshData()
    {
        using (ViewManager.BeginLoad("Retrieving Contracts..."))
        {
            var result = await VenlyAPI.Token.GetErc1155Contracts();

            if (result.Success)
            {
                _contractList = result.Data.ToList();
                _contractList.RemoveAll(c => c.OnChainStatus != eVyStatus.Succeeded);

                //Store to cache
                ViewManager.SetGlobalBlackboardData(ApiExplorer_GlobalKeys.DATA_CachedErc1155Contracts, _contractList);
            }
            else ViewManager.HandleException(result.Exception);
        }
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_contractList, "contractList")) return;
        _lstContracts.SetItemSource(_contractList);
    }
    #endregion

    #region EVENTS
    private void onClick_Contract(VyErc1155ContractSummaryDto contract)
    {
        if (IsSelectionMode)
        {
            FinishSelection(contract);
            return;
        }

        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_Erc1155ContractDetails, ApiExplorer_ContractDetailsVC.KEY_ContractAddr, contract.Address);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_Erc1155ContractDetails, ApiExplorer_ContractDetailsVC.KEY_ContractChain, contract.Chain);
        ViewManager.SwitchView(eApiExplorerViewId.TokenApi_Erc1155ContractDetails, ViewId, false);
    }
    #endregion
}
