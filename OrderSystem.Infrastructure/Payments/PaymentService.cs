namespace OrderSystem.Infrastructure.Payments;

public class PaymentService : IPaymentService
{
    public async Task<bool> ProcessPaymentAsync(
        Guid orderId,
        decimal amount,
        CancellationToken cancellationToken
    )
    {
        await Task.Delay(500, cancellationToken);

        return amount > 0;
    }
}
