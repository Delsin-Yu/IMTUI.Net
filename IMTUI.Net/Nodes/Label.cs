namespace IMTUI.Nodes;

internal class Label : Control
{
    public string Text { get; set; } = "";
    public ConsoleColor Color { get; set; }

    public override void Layout()
    {
        var lines = 0;
        var maxLineWidth = 0;
        foreach (var line in Text.EnumerateLines())
        {
            lines++;
            if (line.Length <= maxLineWidth) continue;
            maxLineWidth = line.Length;
        }
        Size = (maxLineWidth, lines);
    }

    public override void Render(Position parentGlobalPosition)
    {
        var lines = 0;
        VirtualConsole.Color = Color;
        foreach (var line in Text.EnumerateLines())
        {
            VirtualConsole.WriteAt((parentGlobalPosition + Position).IncrementY(lines), line);
            lines++;
        }
    }
}