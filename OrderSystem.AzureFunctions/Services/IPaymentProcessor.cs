namespace OrderSystem.AzureFunctions.Services;

public interface IPaymentProcessor
{
    Task ProcessAsync(string message, CancellationToken cancellationToken);
}
