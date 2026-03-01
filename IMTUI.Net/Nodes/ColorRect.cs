namespace IMTUI.Nodes;

public enum BoxRenderingStyle
{
    ThinWireFrame,
    ThickWireFrame,
    DoubleWireFrame,
    Filled,
}

internal class ColorRect : Control
{
    public ConsoleColor Color { get; set; }
    public BoxRenderingStyle Style { get; set; }

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
    
    protected override void OnRender(Position globalPosition)
    {
        VirtualConsole.Color = Color;
        var style = StyleLibrary[Style];
        if (Size == Size.One)
        {
            VirtualConsole.WriteAt(globalPosition, '▪');
        }
        else if (Size.Height == 1)
        {
            VirtualConsole.WriteAt(globalPosition, style[0][0]);
            VirtualConsole.WriteAt(globalPosition.IncrementX(1), style[0][1], Size.Width - 2);
            VirtualConsole.WriteAt(globalPosition.IncrementX(Size.Width - 1), style[0][2]);
        }
        else if (Size.Width == 1)
        {
            VirtualConsole.WriteAt(globalPosition, style[1][0]);
            for (var y = 1; y < Size.Height - 1; y++) VirtualConsole.WriteAt(globalPosition.IncrementY(y), style[1][1]);
            VirtualConsole.WriteAt(globalPosition.IncrementY(Size.Height - 1), style[1][2]);
        }
        else
        {
            VirtualConsole.WriteAt(globalPosition, style[2][0]);
            VirtualConsole.WriteAt(globalPosition.IncrementX(1), style[2][1], Size.Width - 2);
            VirtualConsole.WriteAt(globalPosition.IncrementX(Size.Width - 1), style[2][2]);
            for (var y = 1; y < Size.Height - 1; y++)
            {
                VirtualConsole.WriteAt(globalPosition.IncrementY(y), style[3][0]);
                VirtualConsole.WriteAt(globalPosition.IncrementY(y).IncrementX(1), style[3][1], Size.Width - 2);
                VirtualConsole.WriteAt(globalPosition.IncrementY(y).IncrementX(Size.Width - 1), style[3][2]);
            }

            VirtualConsole.WriteAt(globalPosition.IncrementY(Size.Height - 1), style[4][0]);
            VirtualConsole.WriteAt(globalPosition.IncrementY(Size.Height - 1).IncrementX(1), style[4][1], Size.Width - 2);
            VirtualConsole.WriteAt(globalPosition.IncrementY(Size.Height - 1).IncrementX(Size.Width - 1), style[4][2]);
        }
    }
}