namespace IMTUI.Nodes.Containers;

internal class PanelContainer : Container
{
    public int ContentPaddingLeft { get; set; } = 1;
    public int ContentPaddingTop { get; set; } = 1;
    public int ContentPaddingRight { get; set; } = 1;
    public int ContentPaddingBottom { get; set; } = 1;

    public override Size GetMinimumSize()
    {
        var contentMinimum = GetOverlayMinimumSize();
        var paddingLeft = Math.Max(0, ContentPaddingLeft);
        var paddingTop = Math.Max(0, ContentPaddingTop);
        var paddingRight = Math.Max(0, ContentPaddingRight);
        var paddingBottom = Math.Max(0, ContentPaddingBottom);

        return (
            contentMinimum.Width + paddingLeft + paddingRight,
            contentMinimum.Height + paddingTop + paddingBottom
        );
    }

    public override void Layout(Size availableSize)
    {
        var normalizedSize = (
            Width: Math.Max(0, availableSize.Width),
            Height: Math.Max(0, availableSize.Height)
        );

        var paddingLeft = Math.Max(0, ContentPaddingLeft);
        var paddingTop = Math.Max(0, ContentPaddingTop);
        var paddingRight = Math.Max(0, ContentPaddingRight);
        var paddingBottom = Math.Max(0, ContentPaddingBottom);

        var contentWidth = Math.Max(0, normalizedSize.Width - paddingLeft - paddingRight);
        var contentHeight = Math.Max(0, normalizedSize.Height - paddingTop - paddingBottom);

        var contentMinX = 0;
        var contentMinY = 0;
        var contentMaxRight = 0;
        var contentMaxBottom = 0;

        foreach (var child in Children)
        {
            child.Position = (0, 0);
            child.Layout((contentWidth, contentHeight));

            if (child.Position.X < contentMinX) contentMinX = child.Position.X;
            if (child.Position.Y < contentMinY) contentMinY = child.Position.Y;
            if (child.Position.X + child.LayoutSize.Width > contentMaxRight) contentMaxRight = child.Position.X + child.LayoutSize.Width;
            if (child.Position.Y + child.LayoutSize.Height > contentMaxBottom) contentMaxBottom = child.Position.Y + child.LayoutSize.Height;

            child.Position = (
                child.Position.X + paddingLeft,
                child.Position.Y + paddingTop
            );
        }

        var desiredWidth = paddingLeft + Math.Max(0, contentMaxRight - contentMinX) + paddingRight;
        var desiredHeight = paddingTop + Math.Max(0, contentMaxBottom - contentMinY) + paddingBottom;

        ClampSizeToAvailableWithGrow((desiredWidth, desiredHeight), normalizedSize);
    }
}