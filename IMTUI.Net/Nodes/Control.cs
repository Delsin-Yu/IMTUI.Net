namespace IMTUI.Nodes;

internal class Control
{
    public Position Position { get; set; }
    internal Size LayoutSize { get; private set; } = Size.Zero;
    protected Size ComputedSize => LayoutSize;
    public Size CustomMinimumSize
    {
        get;
        set => field = (
            Math.Max(0, value.Width),
            Math.Max(0, value.Height)
        );
    } = Size.Zero;
    public GrowDirection GrowHorizontal { get; set; } = GrowDirection.End;
    public GrowDirection GrowVertical { get; set; } = GrowDirection.End;
    public ControlSizeFlags SizeFlagsHorizontal { get; set; } = ControlSizeFlags.Fill;
    public ControlSizeFlags SizeFlagsVertical { get; set; } = ControlSizeFlags.Fill;
    public float SizeFlagsStretchRatio
    {
        get;
        set => field = Math.Max(0f, value);
    } = 1f;

    public virtual Size GetMinimumSize() => Size.Zero;

    protected virtual Size GetMinimumSizeForAvailable(Size availableSize) => GetMinimumSize();

    public Size GetCombinedMinimumSize()
    {
        var intrinsicMinimum = GetMinimumSize();
        return (
            Math.Max(0, Math.Max(intrinsicMinimum.Width, CustomMinimumSize.Width)),
            Math.Max(0, Math.Max(intrinsicMinimum.Height, CustomMinimumSize.Height))
        );
    }

    public Size GetCombinedMinimumSizeForAvailable(Size availableSize)
    {
        var intrinsicMinimum = GetMinimumSizeForAvailable(availableSize);
        return (
            Math.Max(0, Math.Max(intrinsicMinimum.Width, CustomMinimumSize.Width)),
            Math.Max(0, Math.Max(intrinsicMinimum.Height, CustomMinimumSize.Height))
        );
    }

    public virtual void Layout(Size availableSize)
    {
        foreach (var child in Children)
        {
            child.Layout(availableSize);
        }

        ClampSizeToAvailableWithGrow(LayoutSize, availableSize);
    }

    public virtual void Render(Position parentGlobalPosition)
    {
        var startCorner = parentGlobalPosition + Position;
        if (LayoutSize != Size.Zero && LayoutSize.Width != 0 && LayoutSize.Height != 0) OnRender(startCorner);

        foreach (var child in Children)
        {
            child.Render(startCorner);
        }
    }

    protected virtual void OnRender(Position globalPosition) { }

    public List<Control> Children { get; } = [];

    protected void ClampSizeToAvailableWithGrow(Size desiredSize, Size availableSize)
    {
        var normalizedAvailable = (
            Width: Math.Max(0, availableSize.Width),
            Height: Math.Max(0, availableSize.Height)
        );

        var combinedMinimum = GetCombinedMinimumSizeForAvailable(normalizedAvailable);

        var normalizedDesired = (
            Width: Math.Max(combinedMinimum.Width, Math.Max(0, desiredSize.Width)),
            Height: Math.Max(combinedMinimum.Height, Math.Max(0, desiredSize.Height))
        );

        var clampedWidth = Math.Max(combinedMinimum.Width, Math.Clamp(normalizedDesired.Width, 0, normalizedAvailable.Width));
        var clampedHeight = Math.Max(combinedMinimum.Height, Math.Clamp(normalizedDesired.Height, 0, normalizedAvailable.Height));

        var overflowX = Math.Max(0, clampedWidth - normalizedAvailable.Width);
        var overflowY = Math.Max(0, clampedHeight - normalizedAvailable.Height);

        Position = (
            Position.X + GrowOffset(overflowX, GrowHorizontal),
            Position.Y + GrowOffset(overflowY, GrowVertical)
        );

        SetLayoutSize((clampedWidth, clampedHeight));
    }

    internal void SetLayoutSize(Size size)
    {
        LayoutSize = (
            Math.Max(0, size.Width),
            Math.Max(0, size.Height)
        );
    }

    protected static int GrowOffset(int overflow, GrowDirection direction)
    {
        if (overflow <= 0) return 0;

        return direction switch
        {
            GrowDirection.Begin => -overflow,
            GrowDirection.Both => -(overflow / 2),
            _ => 0,
        };
    }
}