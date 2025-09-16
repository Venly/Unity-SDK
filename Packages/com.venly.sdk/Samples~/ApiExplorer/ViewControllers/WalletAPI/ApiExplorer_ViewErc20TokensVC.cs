using Venly;
using Venly.Core;
using Venly.Models.Shared;
using Venly.Models.Wallet;

public class ApiExplorer_ViewErc20TokensVC : ApiExplorer_ViewTokensBaseVC<VyErc20TokenDto, VyControl_Erc20TokenListView
    , VyControl_Erc20TokenListItem>
{
    public ApiExplorer_ViewErc20TokensVC() : base(eApiExplorerViewId.WalletApi_ViewErc20Tokens) { }

    protected override eApiExplorerViewId DetailViewId => eApiExplorerViewId.Shared_Erc20TokenDetails;

    protected override VyTask<VyErc20TokenDto[]> GetTokenBalances(string walletId)
    {
        return VenlyAPI.Wallet.GetErc20Tokens(walletId);
    }
}