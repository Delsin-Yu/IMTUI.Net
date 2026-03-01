namespace IMTUI.Nodes;

internal class Label : Control
{
    public string Text { get; set; } = "";
    public ConsoleColor Color { get; set; }

    private readonly List<string> _wrappedLines = [];

    public override Size GetMinimumSize()
    {
        var lineCount = 0;
        var maxLineWidth = 0;

        foreach (var line in Text.EnumerateLines())
        {
            lineCount++;
            var lineLength = line.Length;
            if (lineLength > maxLineWidth) maxLineWidth = lineLength;
        }

        return (maxLineWidth, lineCount);
    }

    protected override Size GetMinimumSizeForAvailable(Size availableSize)
    {
        var maxWidth = Math.Max(0, availableSize.Width);
        if (maxWidth == 0) return (0, 0);

        var measured = MeasureWrapped(maxWidth, int.MaxValue, destination: null);
        return measured;
    }

    public override void Layout(Size availableSize)
    {
        _wrappedLines.Clear();

        Position = (0, 0);

        var maxWidth = Math.Max(0, availableSize.Width);
        var measured = MeasureWrapped(maxWidth, int.MaxValue, _wrappedLines);
        var maxLineWidth = measured.Width;

        ClampSizeToAvailableWithGrow((maxLineWidth, _wrappedLines.Count), availableSize);
    }

    private Size MeasureWrapped(int maxWidth, int maxHeight, List<string>? destination)
    {
        var safeWidth = Math.Max(0, maxWidth);
        var safeHeight = Math.Max(0, maxHeight);

        if (safeWidth == 0 || safeHeight == 0)
        {
            return (0, 0);
        }

        var lineCount = 0;
        var maxLineWidth = 0;

        foreach (var line in Text.EnumerateLines())
        {
            if (lineCount >= safeHeight) break;

            var source = line.ToString();
            if (source.Length == 0)
            {
                destination?.Add(string.Empty);
                lineCount++;
                continue;
            }

            var start = 0;
            while (start < source.Length)
            {
                if (lineCount >= safeHeight) break;

                var remaining = source.Length - start;
                if (remaining <= safeWidth)
                {
                    var wrapped = source[start..];
                    destination?.Add(wrapped);
                    if (wrapped.Length > maxLineWidth) maxLineWidth = wrapped.Length;
                    lineCount++;
                    break;
                }

                var breakAt = -1;
                for (var i = safeWidth - 1; i >= 0; i--)
                {
                    if (!char.IsWhiteSpace(source[start + i])) continue;
                    breakAt = i;
                    break;
                }

                if (breakAt > 0)
                {
                    var wrapped = source.Substring(start, breakAt).TrimEnd();
                    destination?.Add(wrapped);
                    if (wrapped.Length > maxLineWidth) maxLineWidth = wrapped.Length;
                    lineCount++;

                    start += breakAt + 1;
                    while (start < source.Length && char.IsWhiteSpace(source[start])) start++;
                    continue;
                }

                var hardWrapped = source.Substring(start, safeWidth);
                destination?.Add(hardWrapped);
                if (hardWrapped.Length > maxLineWidth) maxLineWidth = hardWrapped.Length;
                lineCount++;

                start += safeWidth;
            }
        }

        return (maxLineWidth, lineCount);
    }

    public override void Render(Position parentGlobalPosition)
    {
        var maxRenderableLines = Math.Min(_wrappedLines.Count, Math.Max(0, LayoutSize.Height));
        VirtualConsole.Color = Color;
        for (var lineIndex = 0; lineIndex < maxRenderableLines; lineIndex++)
        {
            VirtualConsole.WriteAt((parentGlobalPosition + Position).IncrementY(lineIndex), _wrappedLines[lineIndex]);
        }
    }
}