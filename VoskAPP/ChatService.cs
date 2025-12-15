using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VoskAPP;

/// <summary>
/// Service responsible for requesting chat completions from OpenRouter (or OpenAI-compatible) endpoint.
/// Extracted from Program to make the logic unit testable without performing real network calls.
/// </summary>
public class ChatService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string Endpoint = "https://openrouter.ai/api/v1/chat/completions";

    public ChatService(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey ?? string.Empty;
    }

    /// <summary>
    /// Builds payload and returns only the content text of the first choice. Returns null on any error.
    /// </summary>
    public async Task<string?> GetChatAsync(string countryCode, CancellationToken token = default)
    {
        try
        {
            // Add Authorization header once (idempotent)
            if (!string.IsNullOrWhiteSpace(_apiKey) && !_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }

            var payload = new
            {
                messages = new[]
                {
                    new { role = "system", content = "根據用戶輸入的國家三碼來給予該國家語言文字回答，推薦的前三個ASUS電腦的EDM電子單宣傳主旨，要給固定格式：三個主旨用,隔開且用[]包起來，不用其他任何說明"},
                    new { role = "user", content = $"國家三碼：{countryCode}" }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };

            using HttpResponseMessage response = await _httpClient.SendAsync(request, token);
            string json = await response.Content.ReadAsStringAsync(token);

            if (!response.IsSuccessStatusCode)
            {
                return null; // Caller can decide how to handle
            }

            var root = JObject.Parse(json);
            return root["choices"]?[0]?["message"]?["content"]?.ToString();
        }
        catch
        {
            return null; // Swallow for testability / simplicity; production code could log
        }
    }
}
