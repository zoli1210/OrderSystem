using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Messaging.Messages;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.Email.Services;

namespace OrderSystem.AzureFunctions.Services;

public class EmailProcessor : IEmailProcessor
{
    private const string PaymentConfirmationEmailType = "PaymentConfirmation";

    private readonly IOrderRepository _orderRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailProcessor> _logger;
    private readonly IEmailNotificationHistoryRepository _emailHistoryRepository;

    public EmailProcessor(
        IOrderRepository orderRepository,
        IEmailService emailService,
        IEmailNotificationHistoryRepository emailHistoryRepository,
        ILogger<EmailProcessor> logger
    )
    {
        _orderRepository = orderRepository;
        _emailService = emailService;
        _emailHistoryRepository = emailHistoryRepository;
        _logger = logger;
    }

    public async Task ProcessAsync(string message, CancellationToken cancellationToken)
    {
        var emailMessage = JsonSerializer.Deserialize<EmailNotificationMessage>(
            message,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (emailMessage is null)
        {
            throw new InvalidOperationException("Invalid email notification message received.");
        }

        if (string.IsNullOrWhiteSpace(emailMessage.EmailType))
        {
            emailMessage.EmailType = PaymentConfirmationEmailType;
        }

        var order = await _orderRepository.GetByIdAsync(emailMessage.OrderId, cancellationToken);

        if (order is null)
        {
            throw new InvalidOperationException(
                $"Order not found. OrderId: {emailMessage.OrderId}"
            );
        }

        var alreadySent = await _emailHistoryRepository.ExistsSentEmailForOrderAsync(
            emailMessage.OrderId,
            emailMessage.EmailType,
            cancellationToken
        );

        if (alreadySent)
        {
            _logger.LogWarning(
                "Email sending skipped because this email type was already sent for this order. OrderId: {OrderId}, Email: {Email}, EmailType: {EmailType}, Subject: {Subject}",
                emailMessage.OrderId,
                emailMessage.CustomerEmail,
                emailMessage.EmailType,
                emailMessage.Subject
            );

            return;
        }

        var emailHistory = new EmailNotificationHistory(
            emailMessage.OrderId,
            emailMessage.CustomerEmail,
            emailMessage.Subject,
            emailMessage.Body,
            emailMessage.EmailType
        );

        await _emailHistoryRepository.AddAsync(emailHistory, cancellationToken);

        try
        {
            await _emailService.SendAsync(emailMessage, cancellationToken);

            emailHistory.MarkAsSent();

            if (IsPaymentConfirmationEmail(emailMessage))
            {
                order.MarkEmailAsSent();
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);

            await _orderRepository.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Email notification processed. OrderId: {OrderId}, Email: {Email}, EmailType: {EmailType}, Subject: {Subject}",
                emailMessage.OrderId,
                emailMessage.CustomerEmail,
                emailMessage.EmailType,
                emailMessage.Subject
            );
        }
        catch (Exception exception)
        {
            emailHistory.MarkAsFailed(exception.Message);

            await _orderRepository.SaveChangesAsync(cancellationToken);

            _logger.LogError(
                exception,
                "Email notification failed. OrderId: {OrderId}, Email: {Email}, EmailType: {EmailType}",
                emailMessage.OrderId,
                emailMessage.CustomerEmail,
                emailMessage.EmailType
            );

            throw;
        }
    }

    private static bool IsPaymentConfirmationEmail(EmailNotificationMessage emailMessage)
    {
        return string.Equals(
            emailMessage.EmailType,
            PaymentConfirmationEmailType,
            StringComparison.OrdinalIgnoreCase
        );
    }
}
