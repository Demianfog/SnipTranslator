using System.Threading.Tasks;

namespace SnipTranslator.MVVM.Translators.Interfaces;

public interface ITranslatorEngine
{
    void SetLanguage(string languageCode);
    Task<string?> TranslateAsync(string text);
}