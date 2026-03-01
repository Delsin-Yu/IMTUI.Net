using IMTUI;
using IMTUI.Nodes;

var ui = new MyUI();
ImmediateModeUIRenderer.Run(ui);

internal class MyUI : IImmediateModeTerminalUI
{
    private int _primaryCounter = 0;
    private int _secondaryCounter = 0;
    private int _tertiaryCounter = 0;
    private bool _useFilledMiddle = false;
    private bool _useDoubleFooter = false;

    public void OnDraw(TerminalUIInstance tui)
    {
        using (tui.VBox(sizeFlagsHorizontal: ControlSizeFlags.ExpandFill, sizeFlagsVertical: ControlSizeFlags.ExpandFill))
        {
            tui.Label("IMTUI.Net - demo", color: ConsoleColor.Cyan, sizeFlagsHorizontal: ControlSizeFlags.ExpandFill);

            using (tui.HBox(sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
            {
                if (tui.Button($"Primary +1 ({_primaryCounter})", style: BoxRenderingStyle.ThinWireFrame, sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
                {
                    _primaryCounter++;
                }

                if (tui.Button($"Secondary +1 ({_secondaryCounter})", style: BoxRenderingStyle.ThickWireFrame, sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
                {
                    _secondaryCounter++;
                }

                if (tui.Button($"Tertiary +1 ({_tertiaryCounter})", style: BoxRenderingStyle.DoubleWireFrame, sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
                {
                    _tertiaryCounter++;
                }
            }

            // Scoped Panel demo.
            using (tui.Panel(sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
            {
                tui.ColorRect(
                    BoxRenderingStyle.ThickWireFrame,
                    color: ConsoleColor.DarkGray,
                    sizeFlagsHorizontal: ControlSizeFlags.ExpandFill,
                    sizeFlagsVertical: ControlSizeFlags.ExpandFill);

                using (tui.VBox(separation: 0, sizeFlagsHorizontal: ControlSizeFlags.ExpandFill, sizeFlagsVertical: ControlSizeFlags.ExpandFill))
                {
                    tui.Label("Scoped Panel + VBox overlay", ConsoleColor.Yellow, sizeFlagsHorizontal: ControlSizeFlags.ExpandFill);
                    tui.Label("Panel overlays children. Last child renders on top.", ConsoleColor.Gray, sizeFlagsHorizontal: ControlSizeFlags.ExpandFill);

                    using (tui.HBox(sizeFlagsHorizontal: ControlSizeFlags.ExpandFill, sizeFlagsVertical: ControlSizeFlags.ExpandFill))
                    {
                        tui.ColorRect(
                            _useFilledMiddle ? BoxRenderingStyle.Filled : BoxRenderingStyle.ThinWireFrame,
                            color: _useFilledMiddle ? ConsoleColor.DarkBlue : ConsoleColor.DarkGreen,
                            minSize: (12, 4),
                            sizeFlagsHorizontal: ControlSizeFlags.ExpandFill,
                            sizeFlagsVertical: ControlSizeFlags.ExpandFill,
                            growHorizontal: GrowDirection.Both,
                            growVertical: GrowDirection.Both);

                        using (tui.VBox(separation: 0, sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
                        {
                            tui.Label("All controls available in TerminalUIInstance:", color: ConsoleColor.White);
                            tui.Label("HBox / VBox / Panel / Label / ColorRect / Button", color: ConsoleColor.Gray);

                            if (tui.Button("Toggle center box style (Thin/Filled)", style: BoxRenderingStyle.ThickWireFrame, sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
                            {
                                _useFilledMiddle = !_useFilledMiddle;
                            }
                        }
                    }
                }
            }

            // Manual Begin/End API demo.
            tui.BeginPanel(sizeFlagsHorizontal: ControlSizeFlags.ExpandFill, sizeFlagsVertical: ControlSizeFlags.Fill);
            tui.ColorRect(
                BoxRenderingStyle.DoubleWireFrame,
                color: ConsoleColor.DarkMagenta,
                minSize: (0, 6),
                sizeFlagsHorizontal: ControlSizeFlags.ExpandFill,
                sizeFlagsVertical: ControlSizeFlags.Fill);

            tui.BeginVBox(separation: 0, sizeFlagsHorizontal: ControlSizeFlags.ExpandFill, sizeFlagsVertical: ControlSizeFlags.Fill);
            tui.Label("Manual Begin*/End* container composition", color: ConsoleColor.Magenta, sizeFlagsHorizontal: ControlSizeFlags.ExpandFill);

            tui.BeginHBox(separation: 1, sizeFlagsHorizontal: ControlSizeFlags.ExpandFill);
            if (tui.Button("Reset counters", style: BoxRenderingStyle.ThinWireFrame, sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
            {
                _primaryCounter = 0;
                _secondaryCounter = 0;
                _tertiaryCounter = 0;
            }

            if (tui.Button("Toggle footer style", style: BoxRenderingStyle.ThickWireFrame, sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
            {
                _useDoubleFooter = !_useDoubleFooter;
            }
            tui.EndHBox();

            tui.EndVBox();
            tui.EndPanel();

            tui.ColorRect(
                _useDoubleFooter ? BoxRenderingStyle.DoubleWireFrame : BoxRenderingStyle.ThinWireFrame,
                color: ConsoleColor.DarkCyan,
                minSize: (0, 3),
                sizeFlagsHorizontal: ControlSizeFlags.ExpandFill,
                sizeFlagsVertical: ControlSizeFlags.Fill,
                growHorizontal: GrowDirection.Both);

            using (tui.HBox(sizeFlagsHorizontal: ControlSizeFlags.ExpandFill))
            {
                tui.Label("Press Tab / Arrow keys to move focus. Enter/Space activates focused button.", ConsoleColor.DarkYellow, sizeFlagsHorizontal: ControlSizeFlags.ExpandFill);

                if (tui.Button("Quit demo", style: BoxRenderingStyle.DoubleWireFrame, color: ConsoleColor.Red, focusedColor: ConsoleColor.Yellow, sizeFlagsHorizontal: ControlSizeFlags.Fill, customMinimumSize: (14, 3)))
                {
                    tui.Finish(false);
                }
            }
        }
    }
}
