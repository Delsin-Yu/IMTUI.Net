namespace IMTUI.Nodes.Containers;

internal class VBoxContainer : Container
{
    public int Separation { get; set; }

    public override Size GetMinimumSize() => GetLinearMinimumSize(Separation, LayoutAxis.Vertical);

    public override void Layout(Size availableSize) =>
        LayoutLinearChildren(availableSize, Separation, LayoutAxis.Vertical);
}