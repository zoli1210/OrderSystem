namespace OrderSystem.AzureFunctions.Services;

public class OrderStatusEmailProcessor
{
    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string EmailType { get; set; } = string.Empty;
}
