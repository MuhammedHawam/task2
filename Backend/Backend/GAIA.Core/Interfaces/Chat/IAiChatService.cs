using GAIA.Core.DTOs.Chat;

namespace GAIA.Core.Interfaces.Chat
{
    public interface IAiChatService
    {
        Task<ChatResponse> GetResponseAsync(string prompt);
    }
}
