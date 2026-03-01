using System.Diagnostics;

namespace IMTUI;

/// <summary>
/// Represents a 2D position in terminal coordinates.
/// </summary>
/// <param name="X">The horizontal coordinate.</param>
/// <param name="Y">The vertical coordinate.</param>
public record struct Position(int X, int Y)
{
    /// <summary>
    /// Converts a tuple to a <see cref="Position"/>.
    /// </summary>
    /// <param name="tuple">The tuple containing X and Y values.</param>
    /// <returns>A <see cref="Position"/> created from the tuple values.</returns>
    [DebuggerStepThrough]
    public static implicit operator Position((int X, int Y) tuple) => new(tuple.X, tuple.Y);

    /// <summary>
    /// Adds two positions component-wise.
    /// </summary>
    /// <param name="a">The first position.</param>
    /// <param name="b">The second position.</param>
    /// <returns>A new <see cref="Position"/> containing the summed coordinates.</returns>
    [DebuggerStepThrough]
    public static Position operator +(Position a, Position b) => (a.X + b.X, a.Y + b.Y);

    /// <summary>
    /// Returns a new position with the X coordinate increased by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to <see cref="X"/>.</param>
    /// <returns>A new <see cref="Position"/> with an updated X value.</returns>
    [DebuggerStepThrough]
    public Position IncrementX(int amount) => (X + amount, Y);

    /// <summary>
    /// Returns a new position with the Y coordinate increased by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to <see cref="Y"/>.</param>
    /// <returns>A new <see cref="Position"/> with an updated Y value.</returns>
    [DebuggerStepThrough]
    public Position IncrementY(int amount) =>  (X, Y + amount);

}

/// <summary>
/// Represents a 2D size in terminal units.
/// </summary>
/// <param name="Width">The width component.</param>
/// <param name="Height">The height component.</param>
public record struct Size(int Width, int Height)
{
    /// <summary>
    /// Gets a size with both dimensions set to 0.
    /// </summary>
    public static readonly Size Zero = (0, 0);

    /// <summary>
    /// Gets a size with both dimensions set to 1.
    /// </summary>
    public static readonly Size One  = (1, 1);

    /// <summary>
    /// Converts a tuple to a <see cref="Size"/>.
    /// </summary>
    /// <param name="tuple">The tuple containing width and height values.</param>
    /// <returns>A <see cref="Size"/> created from the tuple values.</returns>
    public static implicit operator Size((int Width, int Height) tuple) => new(tuple.Width, tuple.Height);
}