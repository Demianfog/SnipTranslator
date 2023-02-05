using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SnipTranslator.Handlers.Keyboard;
using SnipTranslator.MVVM.Handlers.Events;

namespace SnipTranslator.MVVM.Handlers;

public class WindowsGlobalKeyboardHook : IGlobalKeyboardHook, IDisposable
{
    private LocalKeyEventHandler KeyDown;
    private LocalKeyEventHandler KeyUp;

    private delegate int CallbackDelegate(int Code, int W, IntPtr L);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct KBDLLHookStruct
    {
        public Int32 vkCode;
        public Int32 scanCode;
        public Int32 flags;
        public Int32 time;
        public Int32 dwExtraInfo;
    }

    [DllImport("user32", CallingConvention = CallingConvention.StdCall)]
    private static extern int SetWindowsHookEx(HookType idHook, CallbackDelegate lpfn, int hInstance, int threadId);

    [DllImport("user32", CallingConvention = CallingConvention.StdCall)]
    private static extern bool UnhookWindowsHookEx(int idHook);

    [DllImport("user32", CallingConvention = CallingConvention.StdCall)]
    private static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern int GetCurrentThreadId();

    public enum HookType : int
    {
        WH_JOURNALRECORD = 0,
        WH_JOURNALPLAYBACK = 1,
        WH_KEYBOARD = 2,
        WH_GETMESSAGE = 3,
        WH_CALLWNDPROC = 4,
        WH_CBT = 5,
        WH_SYSMSGFILTER = 6,
        WH_MOUSE = 7,
        WH_HARDWARE = 8,
        WH_DEBUG = 9,
        WH_SHELL = 10,
        WH_FOREGROUNDIDLE = 11,
        WH_CALLWNDPROCRET = 12,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }

    private int HookID = 0;
    CallbackDelegate TheHookCB = null;

    //Start hook
    public WindowsGlobalKeyboardHook()
    {
        TheHookCB = KeybHookProc;
        HookID = SetWindowsHookEx(HookType.WH_KEYBOARD_LL, TheHookCB,
            0, //0 for local hook. eller hwnd til user32 for global
            0); //0 for global hook. eller thread for hooken
    }

    bool IsFinalized = false;
    ~WindowsGlobalKeyboardHook()
    {
        Debug.WriteLine($"{DateTime.Now} - Finalized {GetType().Name}");
        if (!IsFinalized)
        {
            UnhookWindowsHookEx(HookID);
            IsFinalized = true;
        }
    }
    
    public void Dispose()
    {
        Debug.WriteLine($"{DateTime.Now} - Disposed {GetType().Name}");
        if (!IsFinalized)
        {
            UnhookWindowsHookEx(HookID);
            IsFinalized = true;
        }
    }

    //The listener that will trigger events
    private int KeybHookProc(int Code, int W, IntPtr L)
    {
        Debug.WriteLine($"{DateTime.Now} - {Code} - {W} - {L}");
        // KBDLLHookStruct LS = new KBDLLHookStruct();
        if (Code < 0)
        {
            return CallNextHookEx(HookID, Code, W, L);
        }

        try
        {
            
            KeyEvents kEvent = (KeyEvents)W;
            Keys vkCode = (Keys)Marshal.ReadInt32(L); //Leser vkCode som er de første 32 bits hvor L peker.

            if (kEvent != KeyEvents.KeyDown && kEvent != KeyEvents.KeyUp && kEvent != KeyEvents.SKeyDown &&
                kEvent != KeyEvents.SKeyUp)
            {
            }

            Debug.WriteLine($"{DateTime.Now} - {vkCode} - {kEvent}");
            if (kEvent == KeyEvents.KeyDown || kEvent == KeyEvents.SKeyDown)
            {
                if (KeyDown != null) KeyDown(vkCode, GetShiftPressed(), GetCtrlPressed(), GetAltPressed());
            }

            if (kEvent == KeyEvents.KeyUp || kEvent == KeyEvents.SKeyUp)
            {
                if (KeyUp != null) KeyUp(vkCode, GetShiftPressed(), GetCtrlPressed(), GetAltPressed());
            }
        }
        catch (Exception)
        {
            //Ignore all errors...
        }
        
        return CallNextHookEx(HookID, Code, W, L);

    }

    public enum KeyEvents
    {
        KeyDown = 0x0100,
        KeyUp = 0x0101,
        SKeyDown = 0x0104,
        SKeyUp = 0x0105
    }

    [DllImport("user32.dll")]
    static public extern short GetKeyState(Keys nVirtKey);

    public static bool GetCapslock()
    {   
        return Convert.ToBoolean(GetKeyState(Keys.CapsLock)) & true;
    }
    public static bool GetNumlock()
    { 
        return Convert.ToBoolean(GetKeyState(Keys.NumLock)) & true;
    }
    public static bool GetScrollLock()
    { 
        return Convert.ToBoolean(GetKeyState(Keys.Scroll)) & true;
    }
    public static bool GetShiftPressed()
    { 
        int state = GetKeyState(Keys.ShiftKey);
        if (state > 1 || state < -1) return true;
        return false;
    }
    public static bool GetCtrlPressed()
    { 
        int state = GetKeyState(Keys.ControlKey);
        if (state > 1 || state < -1) return true;
        return false;
    }
    public static bool GetAltPressed()
    { 
        int state = GetKeyState(Keys.Menu);
        if (state > 1 || state < -1) return true;
        return false;
    }

    public void SetKeyDown(LocalKeyEventHandler eventHandler)
    {
        KeyDown = eventHandler;
    }

    public void SetKeyUp(LocalKeyEventHandler eventHandler)
    {
        KeyUp = eventHandler;
    }
}