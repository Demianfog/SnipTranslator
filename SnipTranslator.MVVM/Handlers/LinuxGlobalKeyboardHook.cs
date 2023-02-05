using System;
using SnipTranslator.MVVM.Handlers.Events;

namespace SnipTranslator.MVVM.Handlers;
public class LinuxGlobalKeyboardHook : IGlobalKeyboardHook
{
    public void SetKeyDown(LocalKeyEventHandler eventHandler)
    {
        throw new NotImplementedException();
    }

    public void SetKeyUp(LocalKeyEventHandler eventHandler)
    {
        throw new NotImplementedException();
    }
}
