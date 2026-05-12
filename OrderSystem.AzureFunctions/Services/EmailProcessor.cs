using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrderSystem.Infrastructure.Messaging.Messages;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.Email.Services;

namespace OrderSystem.AzureFunctions.Services;

public class EmailProcessor : IEmailProcessor
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailProcessor> _logger;

    public EmailProcessor(
        IOrderRepository orderRepository,
        IEmailService emailService,
        ILogger<EmailProcessor> logger
    )
    {
        _orderRepository = orderRepository;
        _emailService = emailService;
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
                "Email sending skipped. Email already sent. OrderId: {OrderId}",
                order.Id
            );

            return;
        }

        await _emailService.SendAsync(emailMessage, cancellationToken);

        order.MarkEmailAsSent();

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        _logger.LogWarning(
            "Email notification processed. OrderId: {OrderId}, Email: {Email}",
            emailMessage.OrderId,
            emailMessage.CustomerEmail
        );
    }
}
