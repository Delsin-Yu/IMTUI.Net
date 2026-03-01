using IMTUI.Nodes;

namespace IMTUI;

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
        ConsoleInput.Initialize();
        var commandDispatcher = new TerminalUIInstance();
        try
        {
            while (!commandDispatcher.Exit)
            {
                commandDispatcher.BeginFrameInput();
                var viewportSize = VirtualConsole.BeginFrame();
                commandDispatcher.Reset(viewportSize);
                immediateModeTerminalUI.OnDraw(commandDispatcher);
                commandDispatcher.Root.Layout(commandDispatcher.Root.LayoutSize);
                commandDispatcher.Root.Render((0, 0));
                VirtualConsole.EndFrame();
                commandDispatcher.EndFrameInput();
            }

            if(commandDispatcher.Clear) Console.Clear();
        }
        finally
        {
            ConsoleInput.Restore();
            Console.CursorVisible = true;
        }
    }
}