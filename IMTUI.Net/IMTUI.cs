using IMTUI.Nodes;
using IMTUI.Nodes.Containers;

namespace IMTUI;

public interface IImmediateModeTerminalUI
{
    void OnDraw(TerminalUIInstance tui);
}

public class TerminalUIInstance
{
    internal bool Exit { get; set; }

    internal Control Root { get; } = new();

    internal void Reset()
    {
        Root.Children.Clear();
        _activeContainerStack.Push(Root);
        Root.Size = (VirtualConsole.Height, VirtualConsole.Width);
    }

    #region InternalAPI

    private readonly Stack<Control> _activeContainerStack = [];

    private class Scope<TContainer>(TContainer container, Stack<Control> controls) : IDisposable where TContainer : Control
    {
        public void Dispose()
        {
            if (!ReferenceEquals(controls.Peek(), container)) throw new InvalidOperationException();
            controls.Pop();
        }
    }

    private void AddChild(Control control) => _activeContainerStack.Peek().Children.Add(control);

    private Scope<TContainer> BeginScoped<TContainer>(TContainer instance) where TContainer : Control
    {
        AddChild(instance);
        _activeContainerStack.Push(instance);
        return new Scope<TContainer>(instance, _activeContainerStack);
    }

    private void BeginContainer<TContainer>(TContainer instance) where TContainer : Control
    {
        AddChild(instance);
        _activeContainerStack.Push(instance);
    }

    private void EndContainer<TContainer>() where TContainer : Control
    {
        if (_activeContainerStack.Peek() is not TContainer) throw new InvalidOperationException();
        _activeContainerStack.Pop();
    }

    #endregion

    public IDisposable HBox(int separation = 1) => BeginScoped(new HBoxContainer { Separation = separation });
    public void BeginHBox(int separation = 1) => BeginContainer(new HBoxContainer { Separation = separation });
    public void EndHBox() => EndContainer<HBoxContainer>();
    public IDisposable VBox(int separation = 1) => BeginScoped(new VBoxContainer { Separation = separation });
    public void BeginVBox(int separation = 1) => BeginContainer(new VBoxContainer { Separation = separation });
    public void EndVBox() => EndContainer<VBoxContainer>();

    public void Box(Size size, BoxRenderingStyle style, ConsoleColor color = ConsoleColor.White) =>
        AddChild(new ColorRect { Size = size, Style = style, Color = color });

    public void Label(string text, ConsoleColor color = ConsoleColor.White) =>
        AddChild(new Label { Text = text, Color = color });
}

public static class ImmediateModeUIRenderer
{
    public static void Run(IImmediateModeTerminalUI immediateModeTerminalUI)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CursorVisible = false;
        Console.Clear();
        var commandDispatcher = new TerminalUIInstance();
        while (!commandDispatcher.Exit)
        {
            commandDispatcher.Reset();
            immediateModeTerminalUI.OnDraw(commandDispatcher);
            commandDispatcher.Root.Render((0, 0));
        }

        Console.Clear();
        Console.CursorVisible = true;
    }
}