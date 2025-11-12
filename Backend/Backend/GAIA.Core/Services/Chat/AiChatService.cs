using System.Text;
using System.Text.Json;
using GAIA.Core.DTOs.Chat;
using GAIA.Core.Interfaces.Chat;
using Microsoft.Extensions.Configuration;

namespace GAIA.Core.Services.Chat;

public class AiChatService : IAiChatService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _url;

    public AiChatService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"];
        _url = configuration["OpenAI:Url"];
    }

    public async Task<ChatResponse> GetResponseAsync(string prompt)
    {
        var requestBody = new
        {
            model = "text-davinci-003",
            prompt,
            max_tokens = 100
        };

        var requestContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(_url),
            Headers =
            {
                { "Authorization", $"Bearer {_apiKey}" }
            },
            Content = requestContent
        };

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"OpenAI API call failed: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        var responseText = jsonResponse
            .GetProperty("output")[0]
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString();
        var reasoningSummary = jsonResponse.GetProperty("reasoning").GetProperty("summary").GetString();

        return new ChatResponse
        {
            Response = responseText,
            ReasoningSummary = reasoningSummary,
        };
    }
}
