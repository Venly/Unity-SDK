using UnityEngine.UIElements;
using Venly.Models.Shared;
using Venly.Models.Token;
using Venly.Utils;

//ITEM
public class VyControl_ContractListItem : VyControl_ListViewItemBase<VyErc1155ContractDto>
{
    //public VyControl_ContractListItem() : base("VyControl_ContractListItem") { }

    public override void GenerateTree(VisualElement root)
    {
        AddLabel(root, "lbl-name", "Name");
        AddLabel(root, "lbl-chain", "Chain");
        AddLabel(root, "lbl-address", "Address");
        AddLabel(root, "lbl-symbol", "Symbol");
    }

    public override void BindItem(VyErc1155ContractDto sourceItem)
    {
        SetLabel("lbl-name", sourceItem.Metadata.Name);
        SetLabel("lbl-chain", sourceItem.Chain.GetMemberName());
        SetLabel("lbl-address", sourceItem.Metadata.ToString());
        SetLabel("lbl-symbol", sourceItem.Metadata.Symbol);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-name", "MockContract");
        SetLabel("lbl-chain", "MATIC");
        SetLabel("lbl-address", "123");
        SetLabel("lbl-symbol", "MCT");
    }
}

//LIST VIEW
public class VyControl_ContractListView : VyControl_ListViewBase<VyErc1155ContractDto, VyControl_ContractListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_ContractListView, UxmlTraits> { }
}
