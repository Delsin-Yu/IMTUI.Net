namespace IMTUI.Nodes.Containers;

internal class HBoxContainer : Control
{
    public int Separation { get; set; }
    public override void Layout()
    {
        var accumulatedWidth = 0;
        var maxHeight = 0;
        foreach (var child in Children)
        {
            child.Layout();
            child.Position = (accumulatedWidth, 0);
            accumulatedWidth += child.Size.Width;
            accumulatedWidth += Separation;
            if (child.Size.Height > maxHeight) maxHeight = child.Size.Height;
        }
    }
}