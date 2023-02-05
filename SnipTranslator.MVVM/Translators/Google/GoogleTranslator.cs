using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SnipTranslator.MVVM.Translators.DTO;
using SnipTranslator.MVVM.Translators.Interfaces;

namespace SnipTranslator.MVVM.Translators.Google;

public class GoogleTranslator : ITranslatorEngine
{
    private RestClient _client = new RestClient("https://translate.google.com");
    private string _languageCode = "en";

    public GoogleTranslator()
    {
        _client.AddDefaultHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        _client.AddDefaultHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
        _client.AddDefaultHeader("Accept-Language", "en-US,en;q=0.9");
        _client.AddDefaultHeader("Accept-Encoding", "gzip, deflate, br");
        _client.AddDefaultHeader("Connection", "keep-alive");
        _client.AddDefaultHeader("Upgrade-Insecure-Requests", "1");
        _client.AddDefaultHeader("Cache-Control", "max-age=0");
        _client.AddDefaultHeader("Sec-Fetch-Site", "none");
        _client.AddDefaultHeader("Sec-Fetch-Mode", "navigate");
        _client.AddDefaultHeader("Sec-Fetch-User", "?1");
        _client.AddDefaultHeader("Sec-Fetch-Dest", "document");
        _client.AddDefaultHeader("Referer", "https://translate.google.com/");
        _client.AddDefaultHeader("Sec-GPC", "1");
        _client.AddDefaultHeader("TE", "Trailers");
        _client.AddDefaultHeader("Host", "translate.google.com");
    }
    public void SetLanguage(string languageCode)
    {
        _languageCode = languageCode;
    }

    public async Task<string?> TranslateAsync(string text)
    {
        RestResponse response = await _client.ExecuteAsync(new RestRequest($"/translate_a/single?client=gtx&sl=auto&tl={_languageCode}&dt=t&q={text}"));
        Translation translation = GetTranslation(response.Content!);
        return translation.TranslatedText;
    }

    private static Translation GetTranslation(string content)
    {
        Translation translation = new Translation();
        JToken jToken = JsonConvert.DeserializeObject<JToken>(content)!;
        translation.TranslatedText = ParseTranslation(jToken);
        translation.OriginalText = ParseTranslation(jToken, 1);
        return translation;
    }

    private static string ParseTranslation(JToken jToken, int textPosition = 0)
    {
        StringBuilder stringBuilder = new StringBuilder();
        JToken root = jToken[0]!; 
        root.Children().ToList().ForEach(x => stringBuilder.Append(x[textPosition]));
        return stringBuilder.ToString();
    }
}