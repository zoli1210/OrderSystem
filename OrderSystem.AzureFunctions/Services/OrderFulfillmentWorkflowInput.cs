namespace OrderSystem.AzureFunctions.Services;

public class OrderFulfillmentWorkflowInput
{
    public Guid OrderId { get; set; }

    public string CustomerEmail { get; set; } = string.Empty;
}
