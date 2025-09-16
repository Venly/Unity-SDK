using UnityEngine.UIElements;
using Venly.Models.Shared;
using Venly.Models.Token;
using Venly.Utils;

//ITEM
public class VyControl_AttributeListItem : VyControl_ListViewItemBase<VyMetadataAttribute>
{
    //public VyControl_AttributeListItem() : base("VyControl_AttributeListItem") { }

    public override void GenerateTree(VisualElement root)
    {
        AddLabel(root, "lbl-type", "Type");
        AddLabel(root, "lbl-name", "Name");
        AddLabel(root, "lbl-value", "Value");
    }

    public override void BindItem(VyMetadataAttribute sourceItem)
    {
        SetLabel("lbl-type", sourceItem.Type);
        SetLabel("lbl-name", sourceItem.Name);
        SetLabel("lbl-value", sourceItem.Value);
    }

    public override void BindMockItem()
    {
        SetLabel("lbl-type", "MockType");
        SetLabel("lbl-name", "MockName");
        SetLabel("lbl-value", "MockValue");
    }
}

//LIST VIEW
public class VyControl_AttributeListView : VyControl_ListViewBase<VyMetadataAttribute, VyControl_AttributeListItem>
{
    public VyControl_AttributeListView() : base(false) { }
    public new class UxmlFactory : UxmlFactory<VyControl_AttributeListView, UxmlTraits> { }
}
