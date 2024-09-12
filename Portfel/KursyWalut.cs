using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class KursyWalut
{
    private readonly string _apiKey = "6dfb78f46c2e437f8e38765a211771cf";
    private readonly HttpClient _httpClient;

    public KursyWalut()
    {
        _httpClient = new HttpClient();

    }

    public async Task<JObject> GetLatestRatesAsync()
    {
        var url = $"https://openexchangerates.org/api/latest.json?app_id={_apiKey}";
        var response = await _httpClient.GetStringAsync(url);
        return JObject.Parse(response);
    }
}