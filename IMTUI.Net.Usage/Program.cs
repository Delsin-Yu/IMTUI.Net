using IMTUI;
using IMTUI.Nodes;

var ui = new MyUI();
ImmediateModeUIRenderer.Run(ui);

internal class MyUI : IImmediateModeTerminalUI
{
    private int _counter = 0;
    private int _counter2 = 0;

    public void OnDraw(TerminalUIInstance tui)
    {
        using (tui.VBox(separation: 0, sizeFlagsVertical: ControlSizeFlags.ExpandFill))
        {
            using (tui.HBox(sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
            {
                if (tui.Button($"Pressed: {_counter} time(s)", sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
                {
                    _counter++;
                }

                if (tui.Button($"Pressed: {_counter2} time(s)", sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
                {
                    _counter2++;
                }
            }

            tui.Label("Lorem ipsum dolor sit amet sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit asper");

            using (tui.HBox(sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
            {
                if (tui.Button($"Pressed: {_counter} time(s)", sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
                {
                    _counter++;
                }

                if (tui.Button($"Pressed: {_counter2} time(s)", sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
                {
                    _counter2++;
                }
            }

            tui.ColorRect(
                BoxRenderingStyle.DoubleWireFrame,
                sizeFlagsHorizontal: ControlSizeFlags.ExpandFill,
                sizeFlagsVertical: ControlSizeFlags.ExpandFill);
            tui.Label("Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur?");

            using (tui.HBox(sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
            {
                if (tui.Button($"Pressed: {_counter} time(s)", sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
                {
                    _counter++;
                }

                if (tui.Button($"Pressed: {_counter2} time(s)", sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
                {
                    _counter2++;
                }
            }

            tui.ColorRect(
                BoxRenderingStyle.ThinWireFrame,
                sizeFlagsHorizontal: ControlSizeFlags.ExpandFill,
                sizeFlagsVertical: ControlSizeFlags.ExpandFill);

            using (tui.HBox(sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
            {
                if (tui.Button($"Pressed: {_counter} time(s)", sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
                {
                    _counter++;
                }

                if (tui.Button($"Pressed: {_counter2} time(s)", sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
                {
                    _counter2++;
                }
            }
        }

        // tui.Finish(false);
    }
}
