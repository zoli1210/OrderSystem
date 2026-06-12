namespace OrderSystem.Application.Orders.Contracts.Requests;

public class CancelOrderRequest
{
    public string Reason { get; set; } = string.Empty;
}
