using Microsoft.AspNetCore.Mvc;
using ExpenseManagement.Services;

namespace ExpenseManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(ChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> PostMessage([FromBody] ChatRequest request)
        {
            try
            {
                var conversationHistory = new List<ChatMessage>();
                
                // Convert history to proper format
                foreach (var msg in request.History ?? new List<HistoryMessage>())
                {
                    conversationHistory.Add(new ChatMessage
                    {
                        Role = msg.Role,
                        Content = msg.Content
                    });
                }

                var response = await _chatService.GetChatResponseAsync(request.Message, conversationHistory);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in chat endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public List<HistoryMessage>? History { get; set; }
    }

    public class HistoryMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
