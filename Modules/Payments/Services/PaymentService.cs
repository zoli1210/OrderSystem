namespace OrderSystem.Modules.Payments.Services
{
    public class PaymentService : IPaymentService
    {
        public async Task<bool> ProcessPaymentAsync(
            Guid orderId,
            decimal amount,
            CancellationToken cancellationToken
        )
        {
            await Task.Delay(500, cancellationToken);

            throw new Exception("Simulated payment provider error");
        }
    }
}
