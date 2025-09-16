using Venly;
using Venly.Core;
using Venly.Models.Shared;
using Venly.Models.Wallet;

public class ApiExplorer_ViewNftsVC : ApiExplorer_ViewTokensBaseVC<VyNftDto, VyControl_NftListView, VyControl_NftListItem>
{
    public ApiExplorer_ViewNftsVC() : base(eApiExplorerViewId.WalletApi_ViewNfts) { }

    protected override eApiExplorerViewId DetailViewId => eApiExplorerViewId.Shared_Erc1155TokenDetails;

    protected override VyTask<VyNftDto[]> GetTokenBalances(string walletId)
    {
        return VenlyAPI.Wallet.GetNfts(walletId);
    }
}
