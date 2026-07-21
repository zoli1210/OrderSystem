namespace OrderSystem.AzureFunctions.Payments.Services;

public interface IPaymentProcessor
{
    Task ProcessAsync(string message, CancellationToken cancellationToken);
}
