using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Messaging.Messages;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.Email.Services;

namespace OrderSystem.AzureFunctions.Services;

public class EmailProcessor : IEmailProcessor
{
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
        var emailMessage = JsonSerializer.Deserialize<EmailNotificationMessage>(message);

        if (emailMessage is null)
        {
            throw new InvalidOperationException("Invalid email notification message received.");
        }

        var order = await _orderRepository.GetByIdAsync(emailMessage.OrderId, cancellationToken);

        if (order is null)
        {
            throw new InvalidOperationException(
                $"Order not found. OrderId: {emailMessage.OrderId}"
            );
        }

        if (order.IsEmailSent())
        {
            _logger.LogWarning(
                "Email sending skipped because email was already sent. OrderId: {OrderId}",
                order.Id
            );

            return;
        }

        var emailHistory = new EmailNotificationHistory(
            emailMessage.OrderId,
            emailMessage.CustomerEmail,
            emailMessage.Subject,
            emailMessage.Body
        );

        await _emailHistoryRepository.AddAsync(emailHistory, cancellationToken);

        try
        {
            await _emailService.SendAsync(emailMessage, cancellationToken);

            emailHistory.MarkAsSent();

            order.MarkEmailAsSent();

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Email notification processed. OrderId: {OrderId}, Email: {Email}",
                emailMessage.OrderId,
                emailMessage.CustomerEmail
            );
        }
        catch (Exception exception)
        {
            emailHistory.MarkAsFailed(exception.Message);

            await _orderRepository.SaveChangesAsync(cancellationToken);

            _logger.LogError(
                exception,
                "Email notification failed. OrderId: {OrderId}, Email: {Email}",
                emailMessage.OrderId,
                emailMessage.CustomerEmail
            );

            throw;
        }
    }
}
