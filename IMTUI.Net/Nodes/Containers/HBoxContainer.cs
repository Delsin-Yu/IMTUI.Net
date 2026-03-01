namespace IMTUI.Nodes.Containers;

internal class HBoxContainer : Container
{
    public int Separation { get; set; }

    public override Size GetMinimumSize() => GetLinearMinimumSize(Separation, LayoutAxis.Horizontal);

    public override void Layout(Size availableSize) =>
        LayoutLinearChildren(availableSize, Separation, LayoutAxis.Horizontal);
}