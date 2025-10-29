using GAIA.Core.DTOs.Chat;
using GAIA.Core.Interfaces.Chat;

namespace GAIA.Core.Services.Chat;

public class MockAiChatService : IAiChatService
{
    private static readonly string[] MockResponses =
    {
        "This is a mock response to your prompt.",
        "Here's a random response for testing purposes.",
        "Mock response generated successfully.",
        "Your prompt has been processed by the mock service."
    };

    public Task<ChatResponse> GetResponseAsync(string prompt)
    {
        var random = new Random();
        var response = MockResponses[random.Next(MockResponses.Length)];

        return Task.FromResult(new ChatResponse
        {
            ReasoningSummary = $"this is a mock reason summary for : {prompt}",
            Response = response
        });
    }
}
