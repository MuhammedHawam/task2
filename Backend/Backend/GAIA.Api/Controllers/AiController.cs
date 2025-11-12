using GAIA.Core.Interfaces.Chat;
using Microsoft.AspNetCore.Mvc;

namespace GAIA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly IAiChatService _aiChatService;

        public AiController(IAiChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> GetChatResponse([FromBody] string prompt)
        {
            var response = await _aiChatService.GetResponseAsync(prompt);
            return Ok(response);
        }
    }
}
