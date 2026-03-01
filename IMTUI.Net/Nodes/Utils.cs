namespace IMTUI.Nodes;

internal class Utils
{
    private static readonly Dictionary<BoxRenderingStyle, string[]> StyleLibrary = new()
    {
        {
            BoxRenderingStyle.ThinWireFrame,
            [
                "╶─╴", "╷│╵",
                "┌─┐",
                "│ │",
                "└─┘",
            ]
        },
        {
            BoxRenderingStyle.ThickWireFrame,
            [
                "╺━╸", "╻┃╹",
                "┏━┓",
                "┃ ┃",
                "┗━┛",
            ]
        },
        {
            BoxRenderingStyle.DoubleWireFrame,
            [
                "╺━╸", "╻┃╹",
                "╔═╗",
                "║ ║",
                "╚═╝",
            ]
        },
        {
            BoxRenderingStyle.Filled,
            [
                "╺━╸", "╻┃╹",
                "▗▄▖",
                "▐█▌",
                "▝▀▘",
            ]
        }
    };

    public static void RenderBox(Position globalPosition, Size size, BoxRenderingStyle style, ConsoleColor color)
    {
       VirtualConsole.Color = color;
        var stylePattern = StyleLibrary[style];
        if (size == Size.One)
        {
            VirtualConsole.WriteAt(globalPosition, '▪');
        }
        else if (size.Height == 1)
        {
            VirtualConsole.WriteAt(globalPosition, stylePattern[0][0]);
            VirtualConsole.WriteAt(globalPosition.IncrementX(1), stylePattern[0][1], size.Width - 2);
            VirtualConsole.WriteAt(globalPosition.IncrementX(size.Width - 1), stylePattern[0][2]);
        }
        else if (size.Width == 1)
        {
            VirtualConsole.WriteAt(globalPosition, stylePattern[1][0]);
            for (var y = 1; y < size.Height - 1; y++) VirtualConsole.WriteAt(globalPosition.IncrementY(y), stylePattern[1][1]);
            VirtualConsole.WriteAt(globalPosition.IncrementY(size.Height - 1), stylePattern[1][2]);
        }
        else
        {
            VirtualConsole.WriteAt(globalPosition, stylePattern[2][0]);
            VirtualConsole.WriteAt(globalPosition.IncrementX(1), stylePattern[2][1], size.Width - 2);
            VirtualConsole.WriteAt(globalPosition.IncrementX(size.Width - 1), stylePattern[2][2]);
            for (var y = 1; y < size.Height - 1; y++)
            {
                VirtualConsole.WriteAt(globalPosition.IncrementY(y), stylePattern[3][0]);
                VirtualConsole.WriteAt(globalPosition.IncrementY(y).IncrementX(1), stylePattern[3][1], size.Width - 2);
                VirtualConsole.WriteAt(globalPosition.IncrementY(y).IncrementX(size.Width - 1), stylePattern[3][2]);
            }

            VirtualConsole.WriteAt(globalPosition.IncrementY(size.Height - 1), stylePattern[4][0]);
            VirtualConsole.WriteAt(globalPosition.IncrementY(size.Height - 1).IncrementX(1), stylePattern[4][1], size.Width - 2);
            VirtualConsole.WriteAt(globalPosition.IncrementY(size.Height - 1).IncrementX(size.Width - 1), stylePattern[4][2]);
        } 
    }
}