using IMTUI;
using IMTUI.Nodes;

var ui = new MyUI();
ImmediateModeUIRenderer.Run(ui);

internal class MyUI : IImmediateModeTerminalUI
{
    private int _counter;

    public void OnDraw(TerminalUIInstance tui)
    {
        using (tui.VBox(
                   sizeFlagsHorizontal: ControlSizeFlags.ExpandFill,
                   sizeFlagsVertical: ControlSizeFlags.ExpandFill))
        {
            tui.Label("Hello IMTUI.Net", color: ConsoleColor.Cyan);

            if (tui.Button($"Count: {_counter}", sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
            {
                _counter++;
            }

            if (tui.Button("Quit", color: ConsoleColor.Red))
            {
                // true flushes the console so no tui junk is left.
                tui.Finish(true);
            }
        }
        tui.Finish(false);
    }
}