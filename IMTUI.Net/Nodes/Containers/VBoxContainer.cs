namespace IMTUI.Nodes.Containers;

internal class VBoxContainer : Control
{
    public int Separation { get; set; }
    public override void Layout()
    {
        var accumulatedHeight = 0;
        var maxWidth = 0;
        foreach (var child in Children)
        {
            child.Layout();
            child.Position = (0, accumulatedHeight);
            accumulatedHeight += child.Size.Height;
            accumulatedHeight += Separation;
            if (child.Size.Width > maxWidth) maxWidth = child.Size.Width;
        }

        Size = (maxWidth, accumulatedHeight);
    }
}