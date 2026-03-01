using IMTUI.Nodes;
using IMTUI.Nodes.Containers;

namespace IMTUI;

/// <summary>
/// Represents a frame-scoped command dispatcher used to compose terminal UI content.
/// </summary>
public class TerminalUIInstance
{
    internal bool Exit { get; set; }
    internal bool Clear { get; set; }

    internal Control Root { get; } = new();

    private int _focusedButtonIndex;
    private int _lastFrameButtonCount;
    private int _currentFrameButtonCount;
    private int _focusDelta;
    private bool _activateFocusedButton;
    private int? _clickedButtonIndex;
    private readonly Dictionary<int, (Position Position, Size Size)> _lastFrameButtonBounds = [];
    private readonly Dictionary<int, (Position Position, Size Size)> _currentFrameButtonBounds = [];

    internal void Reset(Size viewportSize)
    {
        Root.Children.Clear();
        _activeContainerStack.Clear();
        _activeContainerStack.Push(Root);
        Root.SetLayoutSize(viewportSize);
        _currentFrameButtonCount = 0;
    }

    internal void BeginFrameInput()
    {
        _focusDelta = 0;
        _activateFocusedButton = false;
        _clickedButtonIndex = null;

        var inputSnapshot = ConsoleInput.Poll();
        _focusDelta = inputSnapshot.FocusDelta;
        _activateFocusedButton = inputSnapshot.ActivateFocused;

        if (inputSnapshot.MouseClickPosition.HasValue)
        {
            var click = inputSnapshot.MouseClickPosition.Value;
            int? clicked = null;
            for (var i = 0; i < _lastFrameButtonCount; i++)
            {
                if (!_lastFrameButtonBounds.TryGetValue(i, out var bounds)) continue;
                if (!Contains(bounds.Position, bounds.Size, click)) continue;
                clicked = i;
            }

            if (clicked.HasValue)
            {
                _clickedButtonIndex = clicked.Value;
                _focusedButtonIndex = clicked.Value;
            }
        }

        if (_lastFrameButtonCount <= 0)
        {
            _focusedButtonIndex = 0;
            return;
        }

        if (_focusDelta == int.MinValue)
        {
            _focusedButtonIndex = 0;
            return;
        }

        if (_focusDelta == int.MaxValue)
        {
            _focusedButtonIndex = _lastFrameButtonCount - 1;
            return;
        }

        if (_focusDelta != 0)
        {
            _focusedButtonIndex = Mod(_focusedButtonIndex + _focusDelta, _lastFrameButtonCount);
        }
    }

    internal void EndFrameInput()
    {
        _currentFrameButtonBounds.Clear();
        CollectButtonBounds(Root);

        _lastFrameButtonBounds.Clear();
        foreach (var (id, bounds) in _currentFrameButtonBounds)
        {
            _lastFrameButtonBounds[id] = bounds;
        }

        _lastFrameButtonCount = _currentFrameButtonCount;
        if (_lastFrameButtonCount <= 0)
        {
            _focusedButtonIndex = 0;
            return;
        }

        _focusedButtonIndex = Math.Clamp(_focusedButtonIndex, 0, _lastFrameButtonCount - 1);
    }

    private static int Mod(int value, int modulus)
    {
        if (modulus <= 0) return 0;
        var remainder = value % modulus;
        return remainder < 0 ? remainder + modulus : remainder;
    }

    private void CollectButtonBounds(Control control)
    {
        if (control is Button button && button.LastRenderedBounds.HasValue)
        {
            _currentFrameButtonBounds[button.ButtonId] = button.LastRenderedBounds.Value;
        }

        foreach (var child in control.Children)
        {
            CollectButtonBounds(child);
        }
    }

    private static bool Contains(Position origin, Size size, Position point)
    {
        if (size.Width <= 0 || size.Height <= 0) return false;

        return point.X >= origin.X
               && point.X < origin.X + size.Width
               && point.Y >= origin.Y
               && point.Y < origin.Y + size.Height;
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

    private static TControl ApplyLayoutOptions<TControl>(
        TControl control,
        ControlSizeFlags sizeFlagsHorizontal,
        ControlSizeFlags sizeFlagsVertical,
        float stretchRatio,
        GrowDirection growHorizontal,
        GrowDirection growVertical,
        Size? customMinimumSize = null)
        where TControl : Control
    {
        control.SizeFlagsHorizontal = sizeFlagsHorizontal;
        control.SizeFlagsVertical = sizeFlagsVertical;
        control.SizeFlagsStretchRatio = stretchRatio;
        control.GrowHorizontal = growHorizontal;
        control.GrowVertical = growVertical;
        if (customMinimumSize.HasValue) control.CustomMinimumSize = customMinimumSize.Value;
        return control;
    }

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
    public IDisposable HBox(
        int separation = 1,
        ControlSizeFlags sizeFlagsHorizontal = ControlSizeFlags.Fill,
        ControlSizeFlags sizeFlagsVertical = ControlSizeFlags.Fill,
        float sizeFlagsStretchRatio = 1f,
        GrowDirection growHorizontal = GrowDirection.End,
        GrowDirection growVertical = GrowDirection.End,
        Size? customMinimumSize = null) =>
        BeginScoped(ApplyLayoutOptions(
            new HBoxContainer { Separation = separation },
            sizeFlagsHorizontal,
            sizeFlagsVertical,
            sizeFlagsStretchRatio,
            growHorizontal,
            growVertical,
            customMinimumSize));

    /// <summary>
    /// Begins a horizontally stacked container.
    /// </summary>
    public void BeginHBox(
        int separation = 1,
        ControlSizeFlags sizeFlagsHorizontal = ControlSizeFlags.Fill,
        ControlSizeFlags sizeFlagsVertical = ControlSizeFlags.Fill,
        float sizeFlagsStretchRatio = 1f,
        GrowDirection growHorizontal = GrowDirection.End,
        GrowDirection growVertical = GrowDirection.End,
        Size? customMinimumSize = null) =>
        BeginContainer(ApplyLayoutOptions(
            new HBoxContainer { Separation = separation },
            sizeFlagsHorizontal,
            sizeFlagsVertical,
            sizeFlagsStretchRatio,
            growHorizontal,
            growVertical,
            customMinimumSize));

    /// <summary>
    /// Ends the most recently opened horizontal container.
    /// </summary>
    public void EndHBox() => EndContainer<HBoxContainer>();

    /// <summary>
    /// Begins a vertically stacked container and returns a scope that ends it on disposal.
    /// </summary>
    public IDisposable VBox(
        int separation = 1,
        ControlSizeFlags sizeFlagsHorizontal = ControlSizeFlags.ExpandFill,
        ControlSizeFlags sizeFlagsVertical = ControlSizeFlags.ExpandFill,
        float sizeFlagsStretchRatio = 1f,
        GrowDirection growHorizontal = GrowDirection.End,
        GrowDirection growVertical = GrowDirection.End,
        Size? customMinimumSize = null) =>
        BeginScoped(ApplyLayoutOptions(
            new VBoxContainer { Separation = separation },
            sizeFlagsHorizontal,
            sizeFlagsVertical,
            sizeFlagsStretchRatio,
            growHorizontal,
            growVertical,
            customMinimumSize));

    /// <summary>
    /// Begins a vertically stacked container.
    /// </summary>
    public void BeginVBox(
        int separation = 1,
        ControlSizeFlags sizeFlagsHorizontal = ControlSizeFlags.ExpandFill,
        ControlSizeFlags sizeFlagsVertical = ControlSizeFlags.ExpandFill,
        float sizeFlagsStretchRatio = 1f,
        GrowDirection growHorizontal = GrowDirection.End,
        GrowDirection growVertical = GrowDirection.End,
        Size? customMinimumSize = null) =>
        BeginContainer(ApplyLayoutOptions(
            new VBoxContainer { Separation = separation },
            sizeFlagsHorizontal,
            sizeFlagsVertical,
            sizeFlagsStretchRatio,
            growHorizontal,
            growVertical,
            customMinimumSize));

    /// <summary>
    /// Ends the most recently opened vertical container.
    /// </summary>
    public void EndVBox() => EndContainer<VBoxContainer>();

    /// <summary>
    /// Begins an overlay panel container and returns a scope that ends it on disposal.
    /// </summary>
    public IDisposable Panel(
        ControlSizeFlags sizeFlagsHorizontal = ControlSizeFlags.Fill,
        ControlSizeFlags sizeFlagsVertical = ControlSizeFlags.Fill,
        float sizeFlagsStretchRatio = 1f,
        GrowDirection growHorizontal = GrowDirection.End,
        GrowDirection growVertical = GrowDirection.End,
        Size? customMinimumSize = null) =>
        BeginScoped(ApplyLayoutOptions(
            new PanelContainer(),
            sizeFlagsHorizontal,
            sizeFlagsVertical,
            sizeFlagsStretchRatio,
            growHorizontal,
            growVertical,
            customMinimumSize));

    /// <summary>
    /// Begins an overlay panel container.
    /// </summary>
    public void BeginPanel(
        ControlSizeFlags sizeFlagsHorizontal = ControlSizeFlags.Fill,
        ControlSizeFlags sizeFlagsVertical = ControlSizeFlags.Fill,
        float sizeFlagsStretchRatio = 1f,
        GrowDirection growHorizontal = GrowDirection.End,
        GrowDirection growVertical = GrowDirection.End,
        Size? customMinimumSize = null) =>
        BeginContainer(ApplyLayoutOptions(
            new PanelContainer(),
            sizeFlagsHorizontal,
            sizeFlagsVertical,
            sizeFlagsStretchRatio,
            growHorizontal,
            growVertical,
            customMinimumSize));

    /// <summary>
    /// Ends the most recently opened panel container.
    /// </summary>
    public void EndPanel() => EndContainer<PanelContainer>();

    /// <summary>
    /// Adds a colored box control to the active container.
    /// </summary>
    public void ColorRect(
        BoxRenderingStyle style,
        ConsoleColor color = ConsoleColor.White,
        Size minSize = default,
        ControlSizeFlags sizeFlagsHorizontal = ControlSizeFlags.Fill,
        ControlSizeFlags sizeFlagsVertical = ControlSizeFlags.Fill,
        float sizeFlagsStretchRatio = 1f,
        GrowDirection growHorizontal = GrowDirection.End,
        GrowDirection growVertical = GrowDirection.End,
        Size? customMinimumSize = null) =>
        AddChild(ApplyLayoutOptions(
            new ColorRect { RequestedMinimumSize = minSize, Style = style, Color = color },
            sizeFlagsHorizontal,
            sizeFlagsVertical,
            sizeFlagsStretchRatio,
            growHorizontal,
            growVertical,
            customMinimumSize));

    /// <summary>
    /// Adds a text label to the active container.
    /// </summary>
    public void Label(
        string text,
        ConsoleColor color = ConsoleColor.White,
        ControlSizeFlags sizeFlagsHorizontal = ControlSizeFlags.Fill,
        ControlSizeFlags sizeFlagsVertical = ControlSizeFlags.Fill,
        float sizeFlagsStretchRatio = 1f,
        GrowDirection growHorizontal = GrowDirection.End,
        GrowDirection growVertical = GrowDirection.End,
        Size? customMinimumSize = null) =>
        AddChild(ApplyLayoutOptions(
            new Label { Text = text, Color = color },
            sizeFlagsHorizontal,
            sizeFlagsVertical,
            sizeFlagsStretchRatio,
            growHorizontal,
            growVertical,
            customMinimumSize));

    /// <summary>
    /// Adds a button control to the active container and returns true when it is activated.
    /// </summary>
    public bool Button(
        string text,
        BoxRenderingStyle style = BoxRenderingStyle.ThinWireFrame,
        ConsoleColor color = ConsoleColor.White,
        ConsoleColor focusedColor = ConsoleColor.Yellow,
        ControlSizeFlags sizeFlagsHorizontal = ControlSizeFlags.Fill,
        ControlSizeFlags sizeFlagsVertical = ControlSizeFlags.Fill,
        float sizeFlagsStretchRatio = 1f,
        GrowDirection growHorizontal = GrowDirection.End,
        GrowDirection growVertical = GrowDirection.End,
        Size? customMinimumSize = null)
    {
        var buttonIndex = _currentFrameButtonCount;
        _currentFrameButtonCount++;

        var isFocused = buttonIndex == _focusedButtonIndex;
        var isActivated = (isFocused && _activateFocusedButton) || _clickedButtonIndex == buttonIndex;

        AddChild(ApplyLayoutOptions(
            new Button
            {
                ButtonId = buttonIndex,
                Text = text,
                Style = style,
                Color = color,
                FocusedColor = focusedColor,
                IsFocused = isFocused,
            },
            sizeFlagsHorizontal,
            sizeFlagsVertical,
            sizeFlagsStretchRatio,
            growHorizontal,
            growVertical,
            customMinimumSize));

        return isActivated;
    }

    public void Finish(bool clearOnExit)
    {
        Exit = true;
        Clear = clearOnExit;
    }
}