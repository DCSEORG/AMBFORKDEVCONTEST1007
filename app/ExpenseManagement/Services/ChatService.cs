using Azure.AI.OpenAI;
using Azure;
using Azure.Identity;
using ExpenseManagement.Models;
using System.Text.Json;

namespace ExpenseManagement.Services
{
    public class ChatService
    {
        private readonly IConfiguration _configuration;
        private readonly DatabaseService _dbService;
        private readonly ILogger<ChatService> _logger;
        private readonly OpenAIClient? _client;
        private readonly string? _deploymentName;

        public ChatService(IConfiguration configuration, DatabaseService dbService, ILogger<ChatService> logger)
        {
            _configuration = configuration;
            _dbService = dbService;
            _logger = logger;

            var endpoint = configuration["OpenAI:Endpoint"];
            _deploymentName = configuration["OpenAI:DeploymentName"];
            var managedIdentityClientId = configuration["ManagedIdentityClientId"];

            if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(_deploymentName))
            {
                try
                {
                    Azure.Core.TokenCredential credential;
                    
                    if (!string.IsNullOrEmpty(managedIdentityClientId))
                    {
                        _logger.LogInformation("Using ManagedIdentityCredential with client ID: {ClientId}", managedIdentityClientId);
                        credential = new ManagedIdentityCredential(managedIdentityClientId);
                    }
                    else
                    {
                        _logger.LogInformation("Using DefaultAzureCredential");
                        credential = new DefaultAzureCredential();
                    }

                    _client = new OpenAIClient(new Uri(endpoint), credential);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize Azure OpenAI client");
                }
            }
        }

        public async Task<string> GetChatResponseAsync(string userMessage, List<ChatMessage> conversationHistory)
        {
            if (_client == null || string.IsNullOrEmpty(_deploymentName))
            {
                return "⚠️ Azure OpenAI is not configured. Please deploy using deploy-with-chat.sh to enable AI chat features.\n\nIn the meantime, you can:\n- View expenses\n- Add new expenses\n- Approve pending expenses\n\nUse the navigation menu above to access these features.";
            }

            try
            {
                var chatCompletionsOptions = new ChatCompletionsOptions()
                {
                    DeploymentName = _deploymentName,
                    Messages =
                    {
                        new ChatRequestSystemMessage(@"You are an AI assistant for an Expense Management System. You help users manage their expenses through natural language.

Available operations:
- View expenses (with filtering)
- Get expense details
- Create new expenses
- Approve expenses
- Get categories

When listing data, always format it nicely with:
- **Bold** for headers and important information
- Use numbered lists (1., 2., 3.) for ordered items
- Use bullet points (-, *) for unordered items  
- Line breaks for readability

Be helpful, concise, and professional.")
                    }
                };

                // Add conversation history
                foreach (var msg in conversationHistory)
                {
                    if (msg.Role == "user")
                    {
                        chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(msg.Content));
                    }
                    else if (msg.Role == "assistant")
                    {
                        chatCompletionsOptions.Messages.Add(new ChatRequestAssistantMessage(msg.Content));
                    }
                }

                // Add current user message
                chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(userMessage));

                Response<ChatCompletions> response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
                ChatCompletions completions = response.Value;

                return completions.Choices[0].Message.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat response");
                return $"Error: {ex.Message}";
            }
        }
    }

    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
