using IMTUI.Nodes;
using IMTUI.Nodes.Containers;

namespace IMTUI;

/// <summary>
/// Defines the immediate-mode entry point used to describe a terminal UI frame.
/// </summary>
public interface IImmediateModeTerminalUI
{
    /// <summary>
    /// Builds the current UI frame by issuing draw commands on the provided <paramref name="tui"/> instance.
    /// </summary>
    /// <param name="tui">The terminal UI command surface for the current frame.</param>
    void OnDraw(TerminalUIInstance tui);
}

/// <summary>
/// Represents a frame-scoped command dispatcher used to compose terminal UI content.
/// </summary>
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

    /// <summary>
    /// Begins a horizontally stacked container and returns a scope that ends it on disposal.
    /// </summary>
    /// <param name="separation">The number of columns to place between child controls.</param>
    /// <returns>An <see cref="IDisposable"/> scope that closes the container when disposed.</returns>
    public IDisposable HBox(int separation = 1) => BeginScoped(new HBoxContainer { Separation = separation });

    /// <summary>
    /// Begins a horizontally stacked container.
    /// </summary>
    /// <param name="separation">The number of columns to place between child controls.</param>
    public void BeginHBox(int separation = 1) => BeginContainer(new HBoxContainer { Separation = separation });

    /// <summary>
    /// Ends the most recently opened horizontal container.
    /// </summary>
    public void EndHBox() => EndContainer<HBoxContainer>();

    /// <summary>
    /// Begins a vertically stacked container and returns a scope that ends it on disposal.
    /// </summary>
    /// <param name="separation">The number of rows to place between child controls.</param>
    /// <returns>An <see cref="IDisposable"/> scope that closes the container when disposed.</returns>
    public IDisposable VBox(int separation = 1) => BeginScoped(new VBoxContainer { Separation = separation });

    /// <summary>
    /// Begins a vertically stacked container.
    /// </summary>
    /// <param name="separation">The number of rows to place between child controls.</param>
    public void BeginVBox(int separation = 1) => BeginContainer(new VBoxContainer { Separation = separation });

    /// <summary>
    /// Ends the most recently opened vertical container.
    /// </summary>
    public void EndVBox() => EndContainer<VBoxContainer>();

    /// <summary>
    /// Adds a colored box control to the active container.
    /// </summary>
    /// <param name="size">The size of the box in rows and columns.</param>
    /// <param name="style">The rendering style used for drawing box characters.</param>
    /// <param name="color">The foreground color used to render the box.</param>
    public void Box(Size size, BoxRenderingStyle style, ConsoleColor color = ConsoleColor.White) =>
        AddChild(new ColorRect { Size = size, Style = style, Color = color });

    /// <summary>
    /// Adds a text label to the active container.
    /// </summary>
    /// <param name="text">The label text to render.</param>
    /// <param name="color">The foreground color used to render the label.</param>
    public void Label(string text, ConsoleColor color = ConsoleColor.White) =>
        AddChild(new Label { Text = text, Color = color });
}

/// <summary>
/// Provides the rendering loop for immediate-mode terminal UIs.
/// </summary>
public static class ImmediateModeUIRenderer
{
    /// <summary>
    /// Runs the UI render loop until the associated <see cref="TerminalUIInstance"/> requests exit.
    /// </summary>
    /// <param name="immediateModeTerminalUI">The immediate-mode UI implementation to draw each frame.</param>
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