using SnipTranslator.Handlers.Keyboard;

namespace SnipTranslator.MVVM.Handlers.Events; 

public delegate void LocalKeyEventHandler(Keys key, bool Shift, bool Ctrl, bool Alt);

public delegate void KeyboardEventHandler(object sender, KeyboardEventArgs e);
