namespace OrderSystem.AzureFunctions.Emails.Services;

public interface IEmailProcessor
{
    Task ProcessAsync(string message, CancellationToken cancellationToken);
}
