using System;
using SnipTranslator.Handlers;
using SnipTranslator.Handlers.Keyboard;
using SnipTranslator.MVVM.Handlers.Enum;
using SnipTranslator.MVVM.Handlers.Events;

namespace SnipTranslator.MVVM.Handlers;

public class KeyboardEventArgs : EventArgs
{
    public readonly Keys Key;
    public readonly bool Shift;
    public readonly bool Ctrl;
    public readonly bool Alt;
    
    public KeyboardEventArgs(Keys key, bool shift, bool ctrl, bool alt)
    {
        Key = key;
        Shift = shift;
        Ctrl = ctrl;
        Alt = alt;
    }
}

public class OSKeyboardHandler
{
    private readonly IGlobalKeyboardHook globalKeyboardHook;
    public event KeyboardEventHandler? KeyDown;
    public OSKeyboardHandler(OSEnumeration osEnumeration = OSEnumeration.Windows)
    {
        globalKeyboardHook = osEnumeration switch
        {
            OSEnumeration.Windows => new WindowsGlobalKeyboardHook(),
            OSEnumeration.Linux => new LinuxGlobalKeyboardHook(),
            OSEnumeration.MacOS => new MacOSGlobalKeyboardHook(),
            _ => throw new ArgumentOutOfRangeException(nameof(osEnumeration), osEnumeration, null)
        };
        globalKeyboardHook.SetKeyDown(OnKeyDown);
    }

    private void OnKeyDown(Keys key, bool shift, bool ctrl, bool alt)
    {
        KeyDown?.Invoke(this, new KeyboardEventArgs(key, shift, ctrl, alt));
    }

}

