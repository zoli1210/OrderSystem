using OrderSystem.Domain.Enums;

namespace OrderSystem.Application.Orders.Contracts.Responses;

public class OrderSummaryResponse
{
    public int TotalOrders { get; set; }

    public IReadOnlyList<OrderStatusSummaryResponse> OrdersByStatus { get; set; } =
        new List<OrderStatusSummaryResponse>();

    public IReadOnlyList<OrderRevenueSummaryResponse> RevenueByCurrency { get; set; } =
        new List<OrderRevenueSummaryResponse>();

    public int FailedPaymentCount { get; set; }

    public int CancelledOrderCount { get; set; }

    public int OrdersCreatedToday { get; set; }

    public int OrdersCreatedLast7Days { get; set; }

    public DateTime GeneratedAtUtc { get; set; }
}

public class OrderStatusSummaryResponse
{
    public OrderStatus Status { get; set; }

    public int Count { get; set; }
}

public class OrderRevenueSummaryResponse
{
    public string Currency { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public decimal AverageOrderValue { get; set; }

    public int OrderCount { get; set; }
}
