using UnityEngine.UIElements;
using Venly.Models.Wallet;

//ITEM
public class VyControl_UserListItem : VyControl_ListViewItemBase<VyUserDto>
{
    public override void GenerateTree(VisualElement root)
    {
        AddLabel(root, "lbl-id", "ID");
        AddLabel(root, "lbl-reference", "Reference");
    }

    public override void BindItem(VyUserDto sourceItem)
    {
        SetLabel("lbl-id", sourceItem.Id);
        SetLabel("lbl-reference", sourceItem.Reference);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-id", "152eb3ac-999b-4eb7-b564-de0ae1faa60d");
        SetLabel("lbl-reference", "wallet user");
    }
}

//LIST VIEW
public class VyControl_UserListView : VyControl_ListViewBase<VyUserDto, VyControl_UserListItem>
{
    public new class UxmlFactory : UxmlFactory<VyControl_UserListView, UxmlTraits> { }
}
