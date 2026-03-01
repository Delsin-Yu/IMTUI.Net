namespace IMTUI.Nodes;

internal class Control
{
    public Position Position { get; set; }
    public Size Size { get; set; }

    public virtual void Layout() { }

    public virtual void Render(Position parentGlobalPosition)
    {
        foreach (var child in Children)
        {
            child.Layout();
        }

        Layout();

        var startCorner = parentGlobalPosition + Position;
        if (Size != Size.Zero && Size.Width != 0 && Size.Height != 0) OnRender(startCorner);

        foreach (var child in Children)
        {
            child.Render(startCorner);
        }
    }

    protected virtual void OnRender(Position globalPosition) { }

    public List<Control> Children { get; } = [];
}