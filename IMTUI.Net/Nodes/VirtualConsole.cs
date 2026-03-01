using System.Buffers;

namespace IMTUI.Nodes;

internal static class VirtualConsole
{
    public static int Width
    {
        get
        {
            var windowWidth = Console.WindowWidth;
            if (field == windowWidth) return field;
            field = windowWidth;
            Console.Clear();
            return field;
        }
    }

    public static int Height
    {
        get
        {
            var windowHeight = Console.WindowHeight;
            if (field == windowHeight) return field;
            field = windowHeight;
            Console.Clear();
            return field;
        }
    }

    public static ConsoleColor Color
    {
        set => Console.ForegroundColor = value;
    }

    public static void WriteAt(Position position, ReadOnlySpan<char> charArray)
    {
        for (var index = 0; index < charArray.Length; index++) 
            WriteAt(position.IncrementX(index), charArray[index]);
    }
    
    public static void WriteAt(Position position, char c)
    {
        var x = position.X;
        var y = position.Y;
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;
        Console.SetCursorPosition(x, y);
        Console.Write(c);
    }

    public static void WriteAt(Position position, char c, int repeat)
    {
        var x = position.X;
        var y = position.Y;
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;
        Console.SetCursorPosition(x, y);
        for (var i = 0; i < repeat; i++)
        {
            Console.Write(c);
            x++;
            if (x >= Width) return;
        }
    }
}