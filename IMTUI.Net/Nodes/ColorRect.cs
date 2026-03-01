namespace IMTUI.Nodes;

public enum BoxRenderingStyle
{
    ThinWireFrame,
    ThickWireFrame,
    DoubleWireFrame,
    Filled,
}

internal class ColorRect : Control
{
    public Size RequestedMinimumSize
    {
        get;
        set => field = (
            Math.Max(0, value.Width),
            Math.Max(0, value.Height)
        );
    } = Size.Zero;

    public ConsoleColor Color { get; set; }
    public BoxRenderingStyle Style { get; set; }

    public override Size GetMinimumSize() => RequestedMinimumSize;


    protected override void OnRender(Position globalPosition)
    {
        Utils.RenderBox(globalPosition, LayoutSize, Style, Color);
    }
}