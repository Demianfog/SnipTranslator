using System.Threading.Tasks;
using SnipTranslator.MVVM.Translators.Interfaces;

namespace SnipTranslator.MVVM.Translators;

public class Translator
{
    private ITranslatorEngine _engine;

    public Translator(ITranslatorEngine engine)
    {
        _engine = engine;
    }
    //Todo to enum
    public void SetTranslator(ITranslatorEngine engine) => _engine = engine;
    
    public void SetLanguage(string languageCode) => _engine.SetLanguage(languageCode);
    
    public Task<string?> TranslateAsync(string text) => _engine.TranslateAsync(text);
}