namespace OrderSystem.AzureFunctions.Orders.Models;

public class OrderFulfillmentWorkflowInput
{
    public Guid OrderId { get; set; }

    public string CustomerEmail { get; set; } = string.Empty;
}
