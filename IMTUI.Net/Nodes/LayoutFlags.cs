namespace IMTUI.Nodes;

[Flags]
public enum ControlSizeFlags
{
    None = 0,
    ShrinkBegin = None,
    Fill = 1 << 0,
    Expand = 1 << 1,
    ShrinkCenter = 1 << 2,
    ShrinkEnd = 1 << 3,
    ExpandFill = Fill | Expand
}

public enum GrowDirection
{
    End,
    Begin,
    Both,
}
