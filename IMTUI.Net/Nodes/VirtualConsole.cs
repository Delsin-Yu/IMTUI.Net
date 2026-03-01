namespace IMTUI.Nodes;

internal static class VirtualConsole
{
    private readonly record struct Cell(char Char, ConsoleColor Color);

    private static readonly Cell EmptyCell = new(' ', ConsoleColor.White);

    private static Cell[] _frontBuffer = [];
    private static Cell[] _backBuffer = [];
    private static bool _hasFrontBuffer;
    private static bool _pendingHardClear;

    private static int _width;
    private static int _height;
    private static ConsoleColor _activeColor = ConsoleColor.White;

    public static int Width => _width;

    public static int Height => _height;

    public static ConsoleColor Color
    {
        set => _activeColor = value;
    }

    public static Size BeginFrame()
    {
        var (width, height) = ReadConsoleSizeSafe();
        EnsureBuffers(width, height);

        Array.Fill(_backBuffer, EmptyCell);
        _activeColor = ConsoleColor.White;

        return (width, height);
    }

    public static void EndFrame()
    {
        if (_width == 0 || _height == 0) return;

        // If the viewport changed during this frame, skip presenting it.
        // We'll redraw for the new size on the next frame.
        if (!TryReadConsoleSize(out var liveWidth, out var liveHeight))
        {
            _hasFrontBuffer = false;
            _pendingHardClear = true;
            return;
        }

        if (liveWidth != _width || liveHeight != _height)
        {
            _hasFrontBuffer = false;
            _pendingHardClear = true;
            return;
        }

        try
        {
            if (_pendingHardClear)
            {
                HardClear();
                _pendingHardClear = false;
            }

            if (!FlushDiff())
            {
                _hasFrontBuffer = false;
                _pendingHardClear = true;
                return;
            }

            // Avoid committing the frame if terminal size changed during present,
            // even when console APIs did not throw.
            if (!TryReadConsoleSize(out var postFlushWidth, out var postFlushHeight))
            {
                _hasFrontBuffer = false;
                _pendingHardClear = true;
                return;
            }

            if (postFlushWidth != _width || postFlushHeight != _height)
            {
                _hasFrontBuffer = false;
                _pendingHardClear = true;
                return;
            }

            SwapBuffers();
            _hasFrontBuffer = true;
        }
        catch (ArgumentOutOfRangeException)
        {
            // The terminal was resized while flushing. We will retry next frame with fresh dimensions.
            _hasFrontBuffer = false;
            _pendingHardClear = true;
        }
        catch (IOException)
        {
            // Some terminals throw IO exceptions during aggressive resize operations.
            _hasFrontBuffer = false;
            _pendingHardClear = true;
        }
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
        if (x < 0 || x >= _width || y < 0 || y >= _height) return;

        _backBuffer[IndexOf(x, y)] = new(c, _activeColor);
    }

    public static void WriteAt(Position position, char c, int repeat)
    {
        var x = position.X;
        var y = position.Y;
        if (repeat <= 0 || x < 0 || x >= _width || y < 0 || y >= _height) return;

        var maxRepeat = Math.Min(repeat, _width - x);
        var startIndex = IndexOf(x, y);
        var cell = new Cell(c, _activeColor);
        for (var i = 0; i < maxRepeat; i++)
        {
            _backBuffer[startIndex + i] = cell;
        }
    }

    private static (int Width, int Height) ReadConsoleSizeSafe()
    {
        return TryReadConsoleSize(out var width, out var height)
            ? (width, height)
            : (_width, _height);
    }

    private static bool TryReadConsoleSize(out int width, out int height)
    {
        try
        {
            width = Math.Max(0, Console.WindowWidth);
            height = Math.Max(0, Console.WindowHeight);
            return true;
        }
        catch (IOException)
        {
            width = _width;
            height = _height;
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            width = _width;
            height = _height;
            return false;
        }
    }

    private static void EnsureBuffers(int width, int height)
    {
        if (width == _width && height == _height && _backBuffer.Length != 0) return;

        _width = width;
        _height = height;
        var length = _width * _height;
        _frontBuffer = new Cell[length];
        _backBuffer = new Cell[length];
        _hasFrontBuffer = false;
        _pendingHardClear = true;
    }

    private static int IndexOf(int x, int y) => y * _width + x;

    private static bool FlushDiff()
    {
        var currentColor = Console.ForegroundColor;
        for (var y = 0; y < _height; y++)
        {
            if (!TryReadConsoleSize(out var liveWidth, out var liveHeight)
                || liveWidth != _width
                || liveHeight != _height)
            {
                return false;
            }

            var rowStart = y * _width;
            var runX = -1;
            var runLength = 0;
            var runColor = ConsoleColor.White;

            for (var x = 0; x < _width; x++)
            {
                var idx = rowStart + x;
                var next = _backBuffer[idx];
                var changed = !_hasFrontBuffer || _frontBuffer[idx] != next;

                if (!changed)
                {
                    if (runLength > 0)
                    {
                        if (!FlushRun(y, runX, runLength, runColor, ref currentColor))
                        {
                            return false;
                        }

                        runLength = 0;
                    }

                    continue;
                }

                if (runLength == 0)
                {
                    runX = x;
                    runLength = 1;
                    runColor = next.Color;
                    continue;
                }

                if (next.Color == runColor)
                {
                    runLength++;
                    continue;
                }

                if (!FlushRun(y, runX, runLength, runColor, ref currentColor))
                {
                    return false;
                }

                runX = x;
                runLength = 1;
                runColor = next.Color;
            }

            if (runLength > 0)
            {
                if (!FlushRun(y, runX, runLength, runColor, ref currentColor))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool FlushRun(int y, int startX, int length, ConsoleColor runColor, ref ConsoleColor currentColor)
    {
        if (!TryReadConsoleSize(out var liveWidth, out var liveHeight)
            || liveWidth != _width
            || liveHeight != _height)
        {
            return false;
        }

        if (currentColor != runColor)
        {
            Console.ForegroundColor = runColor;
            currentColor = runColor;
        }

        var startIndex = IndexOf(startX, y);
        for (var i = 0; i < length; i++)
        {
            if (!TryReadConsoleSize(out liveWidth, out liveHeight)
                || liveWidth != _width
                || liveHeight != _height)
            {
                return false;
            }

            var x = startX + i;
            if (x < 0 || x >= _width || y < 0 || y >= _height) return false;

            Console.SetCursorPosition(x, y);
            Console.Write(_backBuffer[startIndex + i].Char);
        }

        return true;
    }

    private static void SwapBuffers()
    {
        (_frontBuffer, _backBuffer) = (_backBuffer, _frontBuffer);
    }

    private static void HardClear()
    {
        try
        {
            Console.SetCursorPosition(0, 0);
            // ANSI clear is generally more reliable during aggressive resize operations.
            // 2J: clear visible screen, 3J: clear scrollback, H: move cursor home.
            Console.Write("\u001b[2J\u001b[3J\u001b[H");
        }
        catch (IOException)
        {
            Console.Clear();
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.Clear();
        }
    }
}