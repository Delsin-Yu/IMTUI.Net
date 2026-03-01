# IMTUI.Net

An immediate-mode terminal UI framework for .NET, with controls and layout behavior inspired by Godot Engine’s GUI system.

IMTUI is a side project and may not be updated often, so use it at your own risk. Our vision is to gradually support as many Terminal-Compatible Godot Controls as possible.

<img width="860" height="625" alt="image" src="https://github.com/user-attachments/assets/8a98fe3e-6c23-4076-a24f-031c570b9643" />

---

## Current Features

### Layout & containers

- `HBoxContainer` / `VBoxContainer` with shared linear layout engine
- `PanelContainer` for overlay/panel composition with content padding
- Size flags:
  - `Fill`
  - `Expand`
  - `ExpandFill`
  - `ShrinkCenter`
  - `ShrinkEnd`
- `SizeFlagsStretchRatio` for weighted expand distribution
- `GrowDirection` (`End`, `Begin`, `Both`) for overflow anchoring
- `customMinimumSize` support

### Controls

- `Label`
- `ColorRect`
- `Button`

### Input

- Keyboard navigation for buttons:
  - `Tab` / Arrow keys: move focus
  - `Home` / `End`: jump focus
  - `Enter` / `Space`: activate focused button
- Mouse click support

---

## Installation

> Package publication instructions will be added once a NuGet package is published.

For now, reference the project directly in your solution:

```xml
<ProjectReference Include="..\IMTUI.Net\IMTUI.Net.csproj" />
```

---

## Quick Start

```csharp
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
    }
}

Run the included usage demo:

```bash
dotnet run --project IMTUI.Net.Usage
```

---

## Project Structure

- `IMTUI.Net/` – core framework
- `IMTUI.Net/Nodes/` – controls, layout flags, and virtual console
- `IMTUI.Net/Nodes/Containers/` – container implementations
- `IMTUI.Net.Usage/` – runnable demo app

---

## MIT License

MIT Licensed under the terms in [LICENSE](LICENSE).
