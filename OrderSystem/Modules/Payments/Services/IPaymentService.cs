namespace OrderSystem.Modules.Payments.Services;

public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(Guid orderId, decimal amount, CancellationToken cancellationToken);
}