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