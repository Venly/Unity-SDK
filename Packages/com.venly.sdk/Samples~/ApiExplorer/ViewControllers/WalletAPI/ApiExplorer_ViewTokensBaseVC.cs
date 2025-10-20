using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Venly.Core;
using Venly.Models.Wallet;

public abstract class ApiExplorer_ViewTokensBaseVC<T, TListView, TListItem> : SampleViewBase<eApiExplorerViewId> 
    where TListView : VyControl_ListViewBase<T, TListItem>
    where TListItem : VyControl_ListViewItemBase<T>
{
    protected ApiExplorer_ViewTokensBaseVC(eApiExplorerViewId viewId) :
        base(viewId)
    { }

    //DATA-KEYS
    public static readonly BlackboardKey<T[]> KEY_TokenList = new ("tokenList");
    public static readonly BlackboardKey<VyWalletDto> KEY_SourceWallet = new ("sourceWallet");

    //DATA
    protected TListView _lstTokens;
    protected List<T> _tokenList = null;
    protected VyWalletDto _sourceWallet = null;

    protected abstract eApiExplorerViewId DetailViewId { get; }
    protected abstract VyTask<T[]> GetTokenBalances(string walletId);
    protected abstract BlackboardKey<T> DetailTokenKey { get; }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        _lstTokens = GetElement<TListView>();
    }

    protected override void OnActivate()
    {
        _sourceWallet = null;
        _tokenList = null;

        //View Parameters
        ShowNavigateBack = true;
        ShowNavigateHome = true;
        ShowRefresh = false; //No Refresh

        _lstTokens.OnItemSelected += onClick_Token;

        if (TryGet(KEY_TokenList, out T[] resultArr))
        {
            _tokenList = resultArr.ToList();
            NoDataRefresh = true;
        } 
        
        if (TryGet(KEY_SourceWallet, out _sourceWallet))
        {
            ShowRefresh = _tokenList == null; //Wallet present, refresh is possible
        }
    }

    protected override void OnDeactivate()
    {
        if (_lstTokens != null) _lstTokens.OnItemSelected -= onClick_Token;
    }

    protected override async Task OnRefreshData()
    {
        using (ViewManager.BeginLoad("Refreshing Tokens..."))
        {
            var result = await GetTokenBalances(_sourceWallet.Id);
            if(result.Success) _tokenList = result.Data.ToList();
            else ViewManager.HandleException(result.Exception);
        }
    }

    protected override void OnRefreshUI()
    {
        if (!ValidateData(_tokenList, "tokenList")) return;

        _lstTokens.SetItemSource(_tokenList);
    }
    #endregion

    #region EVENTS
    private void onClick_Token(T token)
    {
        if (IsSelectionMode)
        {
            FinishSelection(token);
            return;
        }

        ViewManager.SetViewBlackboardData(DetailViewId, DetailTokenKey, token);

        if (TryGet(KEY_SourceWallet, out var wallet))
        {
            ViewManager.SetViewBlackboardData(DetailViewId, ApiExplorer_TokenDetailsDataKeys.KEY_WALLET, wallet);
        }

        ViewManager.SwitchView(DetailViewId, ViewId, false);
    }
    #endregion
}