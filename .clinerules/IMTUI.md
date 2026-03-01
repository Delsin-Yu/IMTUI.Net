## IMTUI.Net Project Context

### Project Goal

Create an Immediate Mode Terminal UI Framework that offers components resembling the Godot GUI.

IMTUI.Net is an **immediate-mode terminal UI framework**. Users implement `IImmediateModeTerminalUI.OnDraw(TerminalUIInstance tui)` and fully re-describe the UI every frame.

### Current Capabilities (high-level)

- **Layout engine**
  - Supports Godot-like size flags (`Fill`, `Expand`, `ExpandFill`, `ShrinkCenter`, `ShrinkEnd`), stretch ratios, and grow directions.
  - Uses a shared container layout engine for `HBoxContainer` and `VBoxContainer`.
  - Includes `PanelContainer` for overlay/panel composition with content padding.
- **Core runtime and interaction**
  - Core API is organized into focused files/types: `IImmediateModeTerminalUI`, `TerminalUIInstance`, and `ImmediateModeUIRenderer`.
  - Includes per-frame input lifecycle hooks and input polling (`ConsoleInput`).
  - Includes interactive `Button` control with keyboard focus/activation and mouse click hit-testing.
  - Uses frame lifecycle around `VirtualConsole.BeginFrame()` / `EndFrame()`.
- **Usage sample**
  - `IMTUI.Net.Usage/Program.cs` demonstrates interactive buttons and size-flag-driven layouts.

### Core Runtime Model (Immediate Mode)

`ImmediateModeUIRenderer.Run(...)` owns the loop and orchestrates both input and rendering lifecycle:

1. Initialize terminal and input (`ConsoleInput.Initialize()`).
2. Every frame:
   - `TerminalUIInstance.BeginFrameInput()` captures keyboard/mouse snapshot and computes button focus/activation intent.
   - `VirtualConsole.BeginFrame()` reads viewport size and resets back buffer.
   - `TerminalUIInstance.Reset(viewportSize)` clears tree/stack and sets root layout size.
   - App `OnDraw(...)` issues frame commands (`HBox`, `VBox`, `Panel`, `Label`, `ColorRect`, `Button`).
   - Root performs `Layout(...)`, then `Render(...)`.
   - `VirtualConsole.EndFrame()` presents only diffs.
   - `TerminalUIInstance.EndFrameInput()` captures button bounds for next-frame mouse hit-testing.
3. On exit, renderer restores input mode/cursor and conditionally clears terminal based on `tui.Finish(clearOnExit)`.

UI tree state is ephemeral per frame; app state must persist in user code fields.

### UI Composition API (TerminalUIInstance)

- Container stack (`_activeContainerStack`) supports scoped or manual composition:
  - Scoped: `using var _ = tui.HBox(...)`, `tui.VBox(...)`, `tui.Panel(...)`.
  - Manual: `BeginHBox/EndHBox`, `BeginVBox/EndVBox`, `BeginPanel/EndPanel`.
- Controls exposed by public API:
  - `Label(text, color, ...layout options)`
  - `ColorRect(style, color, minSize, ...layout options)`
  - `Button(text, style, color, focusedColor, ...layout options) -> bool`
- Frame termination:
  - `Finish(bool clearOnExit)` requests loop exit and whether to clear console.

Most composition methods now accept layout options:

- `ControlSizeFlags` (per axis): `Fill`, `Expand`, `ExpandFill`, `ShrinkCenter`, `ShrinkEnd`.
- `SizeFlagsStretchRatio` for weighted expand distribution.
- `GrowDirection` (`End`, `Begin`, `Both`) for overflow anchoring.
- `customMinimumSize` to override intrinsic minimums.

### Layout System (Node-based)

- Base `Control` now tracks:
  - `Position`
  - `LayoutSize` (computed frame size)
  - `Children`
  - Minimum-size pipeline (`GetMinimumSize`, `GetMinimumSizeForAvailable`, custom minimum combination)
- Shared container engine (`Container`) provides:
  - Linear layout (`HBoxContainer`, `VBoxContainer`) with separation and free-space distribution among expanders.
  - Overlay layout (`PanelContainer`) with configurable content padding.
- Size flags are respected during slot allocation and child positioning.
- `Label` now performs wrapping during layout, preferring whitespace breaks with hard-wrap fallback.

### Input & Interaction Model

- `ConsoleInput` polls per frame:
  - Keyboard: Tab/arrow keys move focus, Home/End jump, Enter/Space activate focused button.
  - Mouse (Windows console mode): click selects/activates a button from last-frame bounds.
  - Non-Windows fallback still supports keyboard via `Console.KeyAvailable`.
- `TerminalUIInstance` assigns deterministic button IDs in draw order each frame.
- `Button` records last rendered bounds for hit-testing and supports focused visual state.

### Rendering System

- All control output goes through `VirtualConsole`.
- `VirtualConsole` maintains front/back `Cell` buffers (`char + color`) and does diff-based presentation.
- Flush writes changed runs grouped by color for fewer console operations.
- Resize safety:
  - Reallocates buffers when dimensions change.
  - Detects mid-frame resize and skips commit.
  - Handles `IOException`/`ArgumentOutOfRangeException` defensively.
  - Uses pending hard-clear strategy to re-sync terminal state.

### Practical Guidance for Future Agent Work

- Keep APIs immediate-mode and frame-declarative; avoid retained widget state in framework internals.
- Prefer adding controls as `Control` subclasses with explicit minimum-size + layout + render behavior.
- Preserve container stack correctness (especially manual Begin/End pairing).
- Route all visual output through `VirtualConsole` (never direct `Console.Write` from controls).
- When touching interaction, preserve per-frame button ID ordering and bounds capture flow (`BeginFrameInput` -> draw -> `EndFrameInput`).
- Validate behavior in `IMTUI.Net.Usage/Program.cs` with a runnable demo.
- For short-lived verification demos, explicitly call `tui.Finish(false)` so automation can regain terminal output.