using System.Runtime.InteropServices;

namespace IMTUI;

internal readonly record struct InputSnapshot(int FocusDelta, bool ActivateFocused, Position? MouseClickPosition);

internal static class ConsoleInput
{
    private const int StdInputHandle = -10;

    private const uint EnableMouseInput = 0x0010;
    private const uint EnableExtendedFlags = 0x0080;
    private const uint DisableQuickEditMode = 0x0000;

    private const short KeyEvent = 0x0001;
    private const short MouseEvent = 0x0002;

    private const uint MouseMoved = 0x0001;
    private const uint DoubleClick = 0x0002;

    private const uint FromLeft1stButtonPressed = 0x0001;

    private static bool _initialized;
    private static bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    private static nint _inputHandle;
    private static uint _originalConsoleMode;
    private static bool _hasOriginalConsoleMode;
    private static bool _leftButtonWasDown;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        if (!_isWindows) return;

        _inputHandle = GetStdHandle(StdInputHandle);
        if (_inputHandle == nint.Zero || _inputHandle == new nint(-1))
        {
            _isWindows = false;
            return;
        }

        if (!GetConsoleMode(_inputHandle, out _originalConsoleMode))
        {
            _isWindows = false;
            return;
        }

        _hasOriginalConsoleMode = true;

        var mode = _originalConsoleMode;
        mode |= EnableMouseInput;
        mode |= EnableExtendedFlags;
        mode &= ~0x0040u; // Disable ENABLE_QUICK_EDIT_MODE.
        mode |= DisableQuickEditMode;

        _ = SetConsoleMode(_inputHandle, mode);
    }

    public static void Restore()
    {
        if (!_initialized) return;
        if (_isWindows && _hasOriginalConsoleMode && _inputHandle != nint.Zero && _inputHandle != new nint(-1))
        {
            _ = SetConsoleMode(_inputHandle, _originalConsoleMode);
        }

        _initialized = false;
        _hasOriginalConsoleMode = false;
    }

    public static InputSnapshot Poll()
    {
        Initialize();

        var focusDelta = 0;
        var activateFocused = false;
        Position? mouseClickPosition = null;

        if (_isWindows && _inputHandle != nint.Zero && _inputHandle != new nint(-1))
        {
            if (GetNumberOfConsoleInputEvents(_inputHandle, out var eventCount) && eventCount > 0)
            {
                var events = new InputRecord[eventCount];
                if (ReadConsoleInput(_inputHandle, events, eventCount, out var readCount) && readCount > 0)
                {
                    for (var i = 0; i < readCount; i++)
                    {
                        var input = events[i];
                        if (input.EventType == KeyEvent)
                        {
                            if (input.KeyEventRecord.bKeyDown == 0) continue;
                            var key = (ConsoleKey)input.KeyEventRecord.wVirtualKeyCode;
                            HandleKey(key, ref focusDelta, ref activateFocused);
                            continue;
                        }

                        if (input.EventType != MouseEvent) continue;

                        var mouse = input.MouseEventRecord;
                        if (mouse.dwEventFlags == MouseMoved) continue;

                        var leftIsDown = (mouse.dwButtonState & FromLeft1stButtonPressed) != 0;
                        var clickTriggered =
                            (mouse.dwEventFlags == 0 && leftIsDown && !_leftButtonWasDown)
                            || mouse.dwEventFlags == DoubleClick;

                        if (clickTriggered)
                        {
                            mouseClickPosition = (mouse.dwMousePosition.X, mouse.dwMousePosition.Y);
                        }

                        _leftButtonWasDown = leftIsDown;
                    }

                    return new InputSnapshot(focusDelta, activateFocused, mouseClickPosition);
                }
            }
        }

        while (Console.KeyAvailable)
        {
            var keyInfo = Console.ReadKey(intercept: true);
            HandleKey(keyInfo.Key, ref focusDelta, ref activateFocused);
        }

        return new InputSnapshot(focusDelta, activateFocused, mouseClickPosition);
    }

    private static void HandleKey(ConsoleKey key, ref int focusDelta, ref bool activateFocused)
    {
        switch (key)
        {
            case ConsoleKey.Tab:
            case ConsoleKey.RightArrow:
            case ConsoleKey.DownArrow:
                focusDelta += 1;
                break;

            case ConsoleKey.LeftArrow:
            case ConsoleKey.UpArrow:
                focusDelta -= 1;
                break;

            case ConsoleKey.Home:
                focusDelta = int.MinValue;
                break;

            case ConsoleKey.End:
                focusDelta = int.MaxValue;
                break;

            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                activateFocused = true;
                break;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputRecord
    {
        [FieldOffset(0)] public short EventType;
        [FieldOffset(4)] public KeyEventRecord KeyEventRecord;
        [FieldOffset(4)] public MouseEventRecord MouseEventRecord;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyEventRecord
    {
        public int bKeyDown;
        public ushort wRepeatCount;
        public ushort wVirtualKeyCode;
        public ushort wVirtualScanCode;
        public ushort UnicodeChar;
        public uint dwControlKeyState;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseEventRecord
    {
        public Coord dwMousePosition;
        public uint dwButtonState;
        public uint dwControlKeyState;
        public uint dwEventFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Coord
    {
        public short X;
        public short Y;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetNumberOfConsoleInputEvents(nint hConsoleInput, out uint lpcNumberOfEvents);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadConsoleInput(
        nint hConsoleInput,
        [Out] InputRecord[] lpBuffer,
        uint nLength,
        out uint lpNumberOfEventsRead);
}