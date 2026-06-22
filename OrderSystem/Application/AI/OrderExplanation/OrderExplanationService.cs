using System.Text;
using Microsoft.Extensions.Options;
using OrderSystem.Application.AI.Providers.OpenAi;
using OrderSystem.Application.AI.Providers.Supabase;
using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Options;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.AI.DTOs;
using OrderSystem.Modules.Auth.Services;

namespace OrderSystem.Application.AI.OrderExplanation;

public class OrderExplanationService : IOrderExplanationService
{
    private const string DefaultQuestion =
        "Explain the current state of this order and mention only the relevant next step if there is one.";

    private readonly IOrderRepository _orderRepository;
    private readonly IOrderStatusHistoryRepository _statusHistoryRepository;
    private readonly IEmailNotificationHistoryRepository _emailHistoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly OpenAiOptions _openAiOptions;
    private readonly ILogger<OrderExplanationService> _logger;

    public OrderExplanationService(
        IOrderRepository orderRepository,
        IOrderStatusHistoryRepository statusHistoryRepository,
        IEmailNotificationHistoryRepository emailHistoryRepository,
        ICurrentUserService currentUserService,
        IEmbeddingService embeddingService,
        IVectorSearchService vectorSearchService,
        IChatCompletionService chatCompletionService,
        IOptions<OpenAiOptions> openAiOptions,
        ILogger<OrderExplanationService> logger
    )
    {
        _orderRepository = orderRepository;
        _statusHistoryRepository = statusHistoryRepository;
        _emailHistoryRepository = emailHistoryRepository;
        _currentUserService = currentUserService;
        _embeddingService = embeddingService;
        _vectorSearchService = vectorSearchService;
        _chatCompletionService = chatCompletionService;
        _openAiOptions = openAiOptions.Value;
        _logger = logger;
    }

    public async Task<ExplainOrderResponse> ExplainAsync(
        Guid orderId,
        ExplainOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            _logger.LogInformation("Starting AI order explanation. OrderId: {OrderId}", orderId);

            var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

            if (order is null)
            {
                _logger.LogWarning(
                    "Order not found during AI explanation. OrderId: {OrderId}",
                    orderId
                );

                throw new Exception("Order not found.");
            }

            EnsureUserCanAccessOrder(order);

            _logger.LogInformation(
                "Order loaded for AI explanation. OrderId: {OrderId}, Status: {Status}",
                order.Id,
                order.Status
            );

            var statusHistory = await _statusHistoryRepository.GetByOrderIdAsync(
                orderId,
                cancellationToken
            );

            _logger.LogInformation(
                "Status history loaded. OrderId: {OrderId}, Count: {Count}",
                orderId,
                statusHistory.Count
            );

            var emailHistory = await _emailHistoryRepository.GetByOrderIdAsync(
                orderId,
                cancellationToken
            );

            _logger.LogInformation(
                "Email history loaded. OrderId: {OrderId}, Count: {Count}",
                orderId,
                emailHistory.Count
            );

            var question = string.IsNullOrWhiteSpace(request.Question)
                ? DefaultQuestion
                : request.Question;

            var retrievalQuery = BuildRetrievalQuery(question, order);

            _logger.LogInformation(
                "Creating embedding for AI order explanation. OrderId: {OrderId}",
                orderId
            );

            var embedding = await _embeddingService.CreateEmbeddingAsync(
                retrievalQuery,
                cancellationToken
            );

            var matchCount = request.MatchCount ?? _openAiOptions.DefaultMatchCount;

            _logger.LogInformation(
                "Searching vector documents for AI order explanation. OrderId: {OrderId}, MatchCount: {MatchCount}",
                orderId,
                matchCount
            );

            var documents = await _vectorSearchService.SearchAsync(
                embedding,
                matchCount,
                cancellationToken
            );

            _logger.LogInformation(
                "Vector documents loaded. OrderId: {OrderId}, Count: {Count}",
                orderId,
                documents.Count
            );

            var orderContext = BuildOrderContext(order, statusHistory, emailHistory);

            _logger.LogInformation("Generating AI order explanation. OrderId: {OrderId}", orderId);

            var answer = await _chatCompletionService.GenerateOrderExplanationAsync(
                question,
                orderContext,
                documents,
                cancellationToken
            );

            return new ExplainOrderResponse
            {
                OrderId = order.Id,
                CurrentStatus = order.Status,
                Answer = answer,
                Sources = documents
                    .Select(document => new KnowledgeSourceResponse
                    {
                        Title = document.Title,
                        Url = document.Url,
                        Similarity = document.Similarity,
                    })
                    .ToList(),
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "AI order explanation failed. OrderId: {OrderId}", orderId);

            throw;
        }
    }

    private void EnsureUserCanAccessOrder(Order order)
    {
        if (_currentUserService.IsAdmin)
        {
            return;
        }

        if (order.CreatedByUserId == _currentUserService.UserId)
        {
            return;
        }

        throw new UnauthorizedAccessException("You are not allowed to access this order.");
    }

    private static string BuildRetrievalQuery(string question, Order order)
    {
        return $"""
            {question}

            Current order status: {order.Status}

            Relevant documentation topics:
            order lifecycle,
            order creation,
            payment pending,
            payment processing,
            paid orders,
            failed payments,
            cancelled orders,
            payment retry,
            email notification,
            asynchronous processing,
            Azure Service Bus,
            Azure Functions,
            message processing,
            dead-letter handling
            """;
    }

    private static string BuildOrderContext(
        Order order,
        IReadOnlyList<OrderStatusHistory> statusHistory,
        IReadOnlyList<EmailNotificationHistory> emailHistory
    )
    {
        var builder = new StringBuilder();

        builder.AppendLine($"OrderId: {order.Id}");
        builder.AppendLine($"CurrentStatus: {order.Status}");
        builder.AppendLine($"CustomerName: {order.CustomerName}");
        builder.AppendLine($"CustomerEmail: {order.CustomerEmail}");
        builder.AppendLine($"TotalAmount: {order.TotalAmount}");
        builder.AppendLine($"Currency: {order.Currency}");
        builder.AppendLine($"CreatedAtUtc: {order.CreatedAtUtc}");
        builder.AppendLine($"UpdatedAtUtc: {order.UpdatedAtUtc}");
        builder.AppendLine($"CreatedByUserId: {order.CreatedByUserId}");
        builder.AppendLine($"CancelledAtUtc: {order.CancelledAtUtc}");
        builder.AppendLine($"CancellationReason: {order.CancellationReason}");
        builder.AppendLine($"EmailSentAtUtc: {order.EmailSentAtUtc}");
        builder.AppendLine($"TrackingNumber: {order.TrackingNumber}");
        builder.AppendLine($"PreparationStartedAtUtc: {order.PreparationStartedAtUtc}");
        builder.AppendLine($"ReadyForShipmentAtUtc: {order.ReadyForShipmentAtUtc}");
        builder.AppendLine($"ShippedAtUtc: {order.ShippedAtUtc}");
        builder.AppendLine($"DeliveredAtUtc: {order.DeliveredAtUtc}");
        builder.AppendLine($"ReturnedAtUtc: {order.ReturnedAtUtc}");

        builder.AppendLine();
        builder.AppendLine("Status history:");

        if (statusHistory.Count == 0)
        {
            builder.AppendLine("- No status history records found.");
        }
        else
        {
            foreach (var history in statusHistory.OrderBy(item => item.ChangedAtUtc))
            {
                builder.AppendLine(
                    $"- {history.FromStatus} → {history.ToStatus} at {history.ChangedAtUtc}, changed by {history.ChangedByUserId}"
                );
            }
        }

        builder.AppendLine();
        builder.AppendLine("Email notification history:");

        if (emailHistory.Count == 0)
        {
            builder.AppendLine("- No email notification records found.");
        }
        else
        {
            foreach (var email in emailHistory.OrderBy(item => item.CreatedAtUtc))
            {
                builder.AppendLine(
                    $"- Recipient: {email.Recipient}, Subject: {email.Subject}, Status: {email.Status}, SentAtUtc: {email.SentAtUtc}, FailedAtUtc: {email.FailedAtUtc}, Error: {email.ErrorMessage}"
                );
            }
        }

        return builder.ToString();
    }
}
