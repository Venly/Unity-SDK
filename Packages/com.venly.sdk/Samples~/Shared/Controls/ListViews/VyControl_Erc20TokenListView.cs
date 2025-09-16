using System.Globalization;
using UnityEngine.UIElements;
using Venly.Models.Wallet;

//ITEM
public class VyControl_Erc20TokenListItem : VyControl_ListViewItemBase<VyErc20TokenDto>
{
    //public VyControl_CryptoTokenListItem() : base("VyControl_CryptoTokenListItem") { }

    public override void GenerateTree(VisualElement root)
    {
        AddLabel(root, "lbl-name", "Name");
        AddLabel(root, "lbl-symbol", "Symbol");
        AddLabel(root, "lbl-balance", "Balance");
        AddLabel(root, "lbl-type", "Type");
    }

    public override void BindItem(VyErc20TokenDto sourceItem)
    {
        SetLabel("lbl-name", sourceItem.Name);
        SetLabel("lbl-symbol", sourceItem.Symbol);
        SetLabel("lbl-balance", sourceItem.Balance?.ToString(CultureInfo.InvariantCulture));
        SetLabel("lbl-type", sourceItem.Type);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-name", "MockToken");
        SetLabel("lbl-symbol", "MTK");
        SetLabel("lbl-balance", "2.5454");
        SetLabel("lbl-type", "ERC 20");
    }
}

//LIST VIEW
public class VyControl_Erc20TokenListView : VyControl_ListViewBase<VyErc20TokenDto, VyControl_Erc20TokenListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_Erc20TokenListView, UxmlTraits> { }
}