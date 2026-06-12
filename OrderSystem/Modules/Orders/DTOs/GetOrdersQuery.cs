using OrderSystem.Domain.Enums;

namespace OrderSystem.Modules.Orders.DTOs;

public class GetOrdersQuery
{
    public OrderStatus? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string SortBy { get; set; } = "createdAtUtc";

    public string SortOrder { get; set; } = "desc";
}
