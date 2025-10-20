using System;
using System.Diagnostics.Contracts;
using UnityEngine.UIElements;
using Venly;
using Venly.Models.Shared;
using Venly.Models.Token;
using Venly.Models.Wallet;

public class ApiExplorer_MintTokenVC : SampleViewBase<eApiExplorerViewId>
{
    //DATA-KEYS
    public static readonly BlackboardKey<VyErc1155ContractDto> KEY_Contract = new ("contract");
    public static readonly BlackboardKey<VyErc1155TokenTypeDto> KEY_TokenType = new ("tokentype");

    //DATA
    private VyErc1155TokenTypeDto _sourceTokenType;
    private VyErc1155ContractDto _sourceContract;

    //UI
    [UIBind("btn-select-contract")] private Button _btnSelectContract;
    [UIBind("btn-select-tokentype")] private Button _btnSelectTokenType;


    public ApiExplorer_MintTokenVC() :
        base(eApiExplorerViewId.TokenApi_MintErc1155Token)
    { }

    #region DATA & UI
    protected override void OnBindElements(VisualElement root)
    {
        base.OnBindElements(root);

        BindButton("btn-select-contract", onClick_SelectContract);
        BindButton("btn-select-tokentype", onClick_SelectTokenType);
        BindButton("btn-select-target", onClick_SelectTarget);
        BindButton("btn-mint", onCick_Mint);
    }

    protected override void OnActivate()
    {
        ShowRefresh = false;
        ShowNavigateHome = false;

        //Check if Source Is Set
        TryGet(KEY_Contract, out _sourceContract);
        _btnSelectContract.ToggleDisplay(_sourceContract == null);
        UpdateSourceContract(_sourceContract);

        //Check if Token is Set
        if (_sourceContract != null)
        {
            TryGet(KEY_TokenType, out _sourceTokenType);
            _btnSelectTokenType.ToggleDisplay(_sourceTokenType == null);
            UpdateSourceTokenType(_sourceTokenType);
        }
    }

    private void UpdateSourceContract(VyErc1155ContractDto sourceContract)
    {
        if (_sourceContract != sourceContract) //new contract, reset token
        {
            _sourceTokenType = null;
        }

        _sourceContract = sourceContract;

        if (_sourceContract == null)
        {
            SetLabel("lbl-source-contract", "select contract");
            SetLabel("lbl-source-tokentype", "select contract");
            SetLabel("lbl-fungible", "select contract");
            SetLabel("lbl-supply", "select contract");
        }
        else
        {
            SetLabel("lbl-source-contract", $"{_sourceContract.Metadata.Name}");
            UpdateSourceTokenType(_sourceTokenType);
        }

        //Hide Select Token button if no wallet is selected yet
        _btnSelectTokenType.ToggleDisplay(_sourceContract != null);
    }

    private void UpdateSourceTokenType(VyErc1155TokenTypeDto sourceTokenType)
    {
        _sourceTokenType = sourceTokenType;

        if (_sourceTokenType == null)
        {
            SetLabel("lbl-source-tokentype", "select type");
            SetLabel("lbl-fungible", "select type");
            SetLabel("lbl-supply", "select type");
        }
        else
        {
            SetLabel("lbl-source-tokentype", $"{_sourceTokenType.Metadata.Name} ({_sourceTokenType.TokenTypeId})");
            SetLabel("lbl-fungible", _sourceTokenType.Metadata.Fungible??false ?"YES":"NO");
            SetLabel("lbl-supply", $"{_sourceTokenType.Supply.Current}/{_sourceTokenType.Supply.Max}");
        }
    }

    protected override void OnDeactivate()
    {
        ClearBlackboardData();

        _sourceContract = null;
        _sourceTokenType = null;
    }
    #endregion

    #region EVENTS
    private void onClick_SelectContract()
    {
        ViewManager.SelectionMode(eApiExplorerViewId.TokenApi_ViewErc1155Contracts, "Select Contract")
            .OnComplete(async result =>
            {
                if (result.Success)
                {
                    var summary = result.Data as VyErc1155ContractSummaryDto;
                    var contractResponse = await VenlyAPI.Token.GetErc1155Contract(summary.Chain, summary.Address);
                    if (contractResponse.Success)
                    {
                        UpdateSourceContract(contractResponse.Data);
                    }
                }
            });
    }

    private void onClick_SelectTokenType()
    {
        ViewManager.ClearViewBlackboardData(eApiExplorerViewId.TokenApi_ViewErc1155TokenTypes);
        ViewManager.SetViewBlackboardData(eApiExplorerViewId.TokenApi_ViewErc1155TokenTypes, ApiExplorer_ViewTokenTypesVC.KEY_Contract, _sourceContract);
        ViewManager.SelectionMode(eApiExplorerViewId.TokenApi_ViewErc1155TokenTypes, "Select TokenType")
            .OnComplete(async result =>
            {
                if (result.Success)
                {
                    var summary = result.Data as VyErc1155TokenTypeSummaryDto;
                    var tokenResponse = await VenlyAPI.Token.GetErc1155TokenType(summary.Chain, summary.ContractAddress, summary.TokenTypeId);
                    if(tokenResponse.Success)
                    {
                        UpdateSourceTokenType(tokenResponse.Data);
                    }         
                }
            });
    }

    private void onClick_SelectTarget()
    {
        ViewManager.SelectionMode(eApiExplorerViewId.WalletApi_ViewWallets, "Select Wallet")
            .OnComplete(result =>
            {
                if (result.Success)
                {
                    var wallet = result.Data as VyWalletDto;
                    SetLabel("txt-target-address", wallet.Address);
                }
            });
    }

    private void onCick_Mint()
    {
        //Validate
        if (!ValidateData(_sourceContract, "contract")) return;
        if (!ValidateData(_sourceTokenType, "tokentype")) return;
        if (!ValidateInput("txt-target-address")) return;
        if (!ValidateInput<int>("txt-amount")) return;

        //Execute
        var reqParams = new VyMintErc1155TokensRequest()
        {
            Destinations = new []
            {
                new VyMintDestinationDto()
                {
                    Address = GetValue("txt-target-address"),
                    Amount = GetValue<int>("txt-amount")
                }
            },
            Chain = _sourceContract.Chain,
            ContractAddress = _sourceContract.Address,
            TokenTypeId = _sourceTokenType.TokenTypeId
        };

        VenlyAPI.Token.MintErc1155Tokens(reqParams)
            .WithLoader(ViewManager, "Minting...")
            .OnSuccess(mintInfo =>
            {
                ViewManager.Info.Show("Token successfully minted!");
                ViewManager.NavigateBack();
            })
            .OnFail(ViewManager.HandleException);
    }
    #endregion
}
