namespace IMTUI.Nodes;

internal class Button : Control
{
    public int ButtonId { get; set; }
    public string Text { get; set; } = string.Empty;
    public ConsoleColor Color { get; set; } = ConsoleColor.White;
    public ConsoleColor FocusedColor { get; set; } = ConsoleColor.Yellow;
    public BoxRenderingStyle Style { get; set; } = BoxRenderingStyle.ThinWireFrame;
    public bool IsFocused { get; set; }
    internal (Position Position, Size Size)? LastRenderedBounds { get; private set; }

    public override Size GetMinimumSize()
    {
        var contentWidth = Math.Max(1, Text.Length);
        // Box borders + one-space padding on each side.
        return (Math.Max(4, contentWidth + 4), 3);
    }

    protected override void OnRender(Position globalPosition)
    {
        LastRenderedBounds = (globalPosition, LayoutSize);

        var color = IsFocused ? FocusedColor : Color;
        var style = IsFocused && Style == BoxRenderingStyle.ThinWireFrame
            ? BoxRenderingStyle.DoubleWireFrame
            : Style;
        Utils.RenderBox(globalPosition, LayoutSize, style, color);

        if (LayoutSize.Width < 3 || LayoutSize.Height < 3) return;

        var innerWidth = LayoutSize.Width - 2;
        if (innerWidth <= 0) return;

        var displayText = Text;
        if (displayText.Length > innerWidth)
        {
            displayText = displayText[..innerWidth];
        }

        var textX = globalPosition.X + 1 + Math.Max(0, (innerWidth - displayText.Length) / 2);
        var textY = globalPosition.Y + (LayoutSize.Height / 2);
        VirtualConsole.Color = color;
        VirtualConsole.WriteAt((textX, textY), displayText);
    }
}