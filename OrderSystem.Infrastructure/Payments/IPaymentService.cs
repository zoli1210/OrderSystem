namespace OrderSystem.Infrastructure.Payments;

public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(
        Guid orderId,
        decimal amount,
        CancellationToken cancellationToken
    );
}
