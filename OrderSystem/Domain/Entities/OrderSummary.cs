using OrderSystem.Domain.Enums;

namespace OrderSystem.Domain.Entities.OrderSummary;

public class OrderSummary
{
    public int TotalOrders { get; set; }

    public IReadOnlyList<OrderStatusCountReadModel> StatusCounts { get; set; } =
        new List<OrderStatusCountReadModel>();

    public IReadOnlyList<OrderRevenueSummaryReadModel> RevenueByCurrency { get; set; } =
        new List<OrderRevenueSummaryReadModel>();

    public int OrdersCreatedToday { get; set; }

    public int OrdersCreatedLast7Days { get; set; }
}

public class OrderStatusCountReadModel
{
    public OrderStatus Status { get; set; }

    public int Count { get; set; }
}

public class OrderRevenueSummaryReadModel
{
    public string Currency { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public decimal AverageOrderValue { get; set; }

    public int OrderCount { get; set; }
}
