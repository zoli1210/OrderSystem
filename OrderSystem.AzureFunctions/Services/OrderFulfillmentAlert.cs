namespace OrderSystem.AzureFunctions.Services;

public class OrderFulfillmentAlert
{
    public Guid OrderId { get; set; }

    public string ExpectedStatus { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
