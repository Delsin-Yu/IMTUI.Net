using System.Diagnostics;

namespace IMTUI;

public record struct Position(int X, int Y)
{
    
    [DebuggerStepThrough]
    public static implicit operator Position((int X, int Y) tuple) => new(tuple.X, tuple.Y);
    
    [DebuggerStepThrough]
    public static Position operator +(Position a, Position b) => (a.X + b.X, a.Y + b.Y);
    
    [DebuggerStepThrough]
    public Position IncrementX(int amount) => (X + amount, Y);
    
    [DebuggerStepThrough]
    public Position IncrementY(int amount) =>  (X, Y + amount);
    
}

public record struct Size(int Width, int Height)
{
    public static readonly Size Zero = (0, 0);
    public static readonly Size One  = (1, 1);
    public static implicit operator Size((int Width, int Height) tuple) => new(tuple.Width, tuple.Height);
}