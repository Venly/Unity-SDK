using System;
using System.Threading.Tasks;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Wallet;

public class ApiExplorer_TransactionDetailsVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEY
    public static readonly BlackboardKey<eVyChain> KEY_TxChain = new BlackboardKey<eVyChain>("tx_chain");
    public static readonly BlackboardKey<string> KEY_TxHash = new BlackboardKey<string>("tx_hash");

    //DATA
    private eVyChain _transactionChain;
    private string _transactionHash;
    private VyTransactionStatusDto _transactionStatus;

    public ApiExplorer_TransactionDetailsVC() : 
        base(eApiExplorerViewId.WalletApi_TransactionDetails) { }

    #region DATA & UI
    protected override void OnActivate()
    {
        ShowNavigateBack = true;
        ShowRefresh = true;
        ShowNavigateHome = false;  

        if (TryGetRaw(KEY_TxChain, out object txChain))
        {
            _transactionChain = (eVyChain)txChain;
        }

        TryGet(KEY_TxHash, out _transactionHash);
    }

    protected async override Task OnRefreshData()
    {
        _transactionStatus = null;

        //Validate
        if (!ValidateData(_transactionChain, "tx_chain")) return;
        if (!ValidateData(_transactionHash, "tx_hash")) return;

        //Execute
        using (ViewManager.BeginLoad("Retrieving Transaction Info..."))
        {
            var result = await VenlyAPI.Wallet.GetTransactionStatus(_transactionChain, _transactionHash);
            if (result.Success) _transactionStatus = result.Data;
            else ViewManager.HandleException(result.Exception);
        }
    }

    protected override void OnRefreshUI()
    {
        //Validate
        if (!ValidateData(_transactionStatus, "tx_status")) return;

        SetLabel("lbl-hash", _transactionStatus.Hash);
        SetLabel("lbl-status", Enum.GetName(typeof(eVyTransactionState), _transactionStatus.Status));
        SetLabel("lbl-confirmations", _transactionStatus.Confirmations.ToString());
        SetLabel("lbl-blockHash", _transactionStatus.BlockHash);
        SetLabel("lbl-blockNumber", _transactionStatus.BlockNumber.ToString());
        SetLabel("lbl-reachedFinality", _transactionStatus.HasReachedFinality ?? false ? "YES" : "NO");
    }
    #endregion
}
