using SnipTranslator.MVVM.Handlers.Events;

namespace SnipTranslator.MVVM.Handlers;

public interface IGlobalKeyboardHook
{
     void SetKeyDown(LocalKeyEventHandler eventHandler);
     void SetKeyUp(LocalKeyEventHandler eventHandler);
}