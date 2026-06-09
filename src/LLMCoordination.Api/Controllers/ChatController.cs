using System.Text.Json;
using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Application.Services;
using LLMCoordination.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LLMCoordination.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ChatOrchestrationService _chatService;
    private readonly TenantContextService _tenantContext;
    private readonly IConversationRepository _conversationRepository;

    public ChatController(
        ChatOrchestrationService chatService,
        TenantContextService tenantContext,
        IConversationRepository conversationRepository)
    {
        _chatService = chatService;
        _tenantContext = tenantContext;
        _conversationRepository = conversationRepository;
    }

    [HttpPost("message")]
    [EnableRateLimiting("chat")]
    public async Task<ActionResult<ChatResponse>> SendMessage(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message is required." });
        }

        if (request.Message.Length > 4000)
        {
            return BadRequest(new { message = "Message must not exceed 4000 characters." });
        }

        var tenantId = _tenantContext.GetCurrentTenantId();
        var userId = _tenantContext.GetCurrentUserId();
        var role = _tenantContext.GetCurrentUserRole();

        if (tenantId is null || userId is null)
        {
            return Unauthorized();
        }

        var conversationId = Guid.TryParse(request.ConversationId, out var parsedId)
            ? parsedId
            : Guid.NewGuid();

        var conversation = await _conversationRepository.GetByIdAsync(conversationId, tenantId.Value, cancellationToken);
        if (conversation is null)
        {
            conversation = await _conversationRepository.CreateAsync(new Conversation
            {
                Id = conversationId,
                TenantId = tenantId.Value,
                UserId = userId.Value,
                Title = request.Message.Length > 80 ? request.Message[..80] : request.Message
            }, cancellationToken);
        }

        await _conversationRepository.AddMessageAsync(new ConversationMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = "user",
            MessageText = request.Message
        }, cancellationToken);

        var context = new AgentContext
        {
            ConversationId = conversationId,
            TenantId = tenantId.Value,
            UserId = userId.Value,
            UserRole = role ?? "User",
            RawMessage = request.Message,
            ViewPreference = request.ViewPreference ?? "text"
        };

        var response = await _chatService.ExecuteAsync(request, context, cancellationToken);

        await _conversationRepository.AddMessageAsync(new ConversationMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = "assistant",
            MessageText = response.Answer,
            ResponseJson = JsonSerializer.Serialize(response)
        }, cancellationToken);

        return Ok(response);
    }

    [HttpPost("approve")]
    [EnableRateLimiting("chat")]
    public async Task<ActionResult<ChatResponse>> Approve(
        [FromBody] ApprovalRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message is required." });
        }

        if (!request.Approved)
        {
            return Ok(new ChatResponse
            {
                ConversationId = request.ConversationId ?? Guid.NewGuid().ToString(),
                MessageId = Guid.NewGuid().ToString("N"),
                Status = "cancelled",
                Answer = "Action was not approved."
            });
        }

        var tenantId = _tenantContext.GetCurrentTenantId();
        var userId = _tenantContext.GetCurrentUserId();
        var role = _tenantContext.GetCurrentUserRole();

        if (tenantId is null || userId is null)
        {
            return Unauthorized();
        }

        var conversationId = Guid.TryParse(request.ConversationId, out var parsedId)
            ? parsedId
            : Guid.NewGuid();

        var chatRequest = new ChatRequest
        {
            ConversationId = conversationId.ToString(),
            Message = request.Message
        };

        var context = new AgentContext
        {
            ConversationId = conversationId,
            TenantId = tenantId.Value,
            UserId = userId.Value,
            UserRole = role ?? "User",
            RawMessage = request.Message,
            ApprovalGranted = true
        };

        var response = await _chatService.ExecuteAsync(chatRequest, context, cancellationToken);
        return Ok(response);
    }
}
