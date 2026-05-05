namespace OrderSystem.AzureFunctions.Services;

public interface IEmailProcessor
{
    Task ProcessAsync(string message, CancellationToken cancellationToken);
}
