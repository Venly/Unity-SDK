using UnityEngine.UIElements;
using Venly.Models;
using Venly.Models.Shared;
using Venly.Models.Wallet;

//ITEM
public class VyControl_NftListItem : VyControl_ListViewItemBase<VyNftDto>
{
    //public VyControl_MultiTokenListItem() : base("VyControl_MultiTokenListItem") { }
    public VyControl_NftListItem() : base() { }

    public override void GenerateTree(VisualElement root)
    {
        AddLabel(root, "lbl-contract", "Contract");
        AddLabel(root, "lbl-id", "Id");
        AddLabel(root, "lbl-fungible", "Fungible");
        AddLabel(root, "lbl-type", "Type");
    }

    public override void BindItem(VyNftDto sourceItem)
    {
        //if (sourceItem.TryGetAttribute("mintNumber", out var attrMintNumber))
        //{
        //    SetLabel("lbl-name", $"{attrMintNumber.Name} (#{attrMintNumber.As<int>()})");
        //}
        //else
        //{
        //    SetLabel("lbl-name", sourceItem.Name);
        //}
       
        SetLabel("lbl-contract", sourceItem.Contract.Name);
        SetLabel("lbl-id", sourceItem.Id);
        SetLabel("lbl-fungible", sourceItem.Fungible);
        SetLabel("lbl-type", sourceItem.Contract.Type);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-contract", "Mock Contract Name");
        SetLabel("lbl-id", "123");
        SetLabel("lbl-fungible", "YES");
        SetLabel("lbl-type", "ERC 1155");
    }
}

//LIST VIEW
public class VyControl_NftListView : VyControl_ListViewBase<VyNftDto, VyControl_NftListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_NftListView, UxmlTraits> { }
}