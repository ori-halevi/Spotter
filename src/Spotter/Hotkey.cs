using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Spotter;

/// <summary>
/// A system-wide hotkey that reaches the app even when it has no focus.
///
/// Uses RegisterHotKey rather than the low-level keyboard hook the 1.5.1 app installed. The hook
/// approach sees every keystroke on the machine and has to filter them itself — a lot of
/// machinery, a permanent presence in every process's input path, and the main reason antivirus
/// looks twice at that binary. RegisterHotKey asks Windows to deliver one specific combination
/// and nothing else.
/// </summary>
public sealed class Hotkey : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int HotkeyId = 0xA51;

    [Flags]
    private enum Modifiers : uint
    {
        Alt = 0x0001,
        Control = 0x0002,
        Shift = 0x0004,
        Win = 0x0008,
        NoRepeat = 0x4000,
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private HwndSource _source;
    private bool _registered;

    /// <summary>Raised on the UI thread when the combination is pressed.</summary>
    public event Action Pressed;

    /// <summary>
    /// Registers the combination. Returns false when another application already owns it —
    /// worth surfacing rather than swallowing, because the symptom is otherwise just "my hotkey
    /// does nothing".
    /// </summary>
    public bool Register(Window window, ModifierKeys modifiers, Key key)
    {
        Unregister();

        IntPtr handle = new WindowInteropHelper(window).EnsureHandle();
        _source = HwndSource.FromHwnd(handle);
        if (_source == null)
        {
            return false;
        }
        _source.AddHook(WndProc);

        uint flags = (uint)Modifiers.NoRepeat;
        if (modifiers.HasFlag(ModifierKeys.Alt)) flags |= (uint)Modifiers.Alt;
        if (modifiers.HasFlag(ModifierKeys.Control)) flags |= (uint)Modifiers.Control;
        if (modifiers.HasFlag(ModifierKeys.Shift)) flags |= (uint)Modifiers.Shift;
        if (modifiers.HasFlag(ModifierKeys.Windows)) flags |= (uint)Modifiers.Win;

        _registered = RegisterHotKey(handle, HotkeyId, flags, (uint)KeyInterop.VirtualKeyFromKey(key));
        if (!_registered)
        {
            Log.Write($"hotkey {modifiers}+{key} could not be registered "
                      + $"(win32 {Marshal.GetLastWin32Error()}) — most likely already taken");
        }
        return _registered;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == HotkeyId)
        {
            Pressed?.Invoke();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Unregister()
    {
        if (_source == null)
        {
            return;
        }
        if (_registered)
        {
            UnregisterHotKey(_source.Handle, HotkeyId);
            _registered = false;
        }
        _source.RemoveHook(WndProc);
        _source = null;
    }

    public void Dispose() => Unregister();

    /// <summary>Parses "Ctrl+Alt+Space" into its parts. Returns false on anything unrecognised.</summary>
    public static bool TryParse(string text, out ModifierKeys modifiers, out Key key)
    {
        modifiers = ModifierKeys.None;
        key = Key.None;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        foreach (string raw in text.Split('+'))
        {
            string part = raw.Trim();
            switch (part.ToLowerInvariant())
            {
                case "ctrl":
                case "control": modifiers |= ModifierKeys.Control; break;
                case "alt": modifiers |= ModifierKeys.Alt; break;
                case "shift": modifiers |= ModifierKeys.Shift; break;
                case "win":
                case "windows": modifiers |= ModifierKeys.Windows; break;
                default:
                    // Enum.TryParse also accepts the *numeric* value of the enum, so "Ctrl+5"
                    // would parse to (Key)5 — Key.Cancel — and register a combination nobody
                    // asked for and nothing in the UI could explain. Only names are valid here.
                    if (part.Length == 0 || char.IsDigit(part[0])
                        || !Enum.TryParse(part, ignoreCase: true, out key)
                        || !Enum.IsDefined(typeof(Key), key))
                    {
                        return false;
                    }
                    break;
            }
        }
        // A bare key with no modifier would swallow that key for the whole machine.
        return key != Key.None && modifiers != ModifierKeys.None;
    }
}
