using SnipTranslator.Handlers.Events;

namespace SnipTranslator.Handlers;

public interface IGlobalKeyboardHook
{
     void SetKeyDown(LocalKeyEventHandler eventHandler);
     void SetKeyUp(LocalKeyEventHandler eventHandler);
}