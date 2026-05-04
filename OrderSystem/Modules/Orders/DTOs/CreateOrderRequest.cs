public class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "HUF";

    public string? Description { get; set; }
}
