using System;
using SnipTranslator.Handlers.Events;

namespace SnipTranslator.Handlers;
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
