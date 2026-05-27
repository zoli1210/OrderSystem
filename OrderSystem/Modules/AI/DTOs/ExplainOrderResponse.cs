using OrderSystem.Domain.Enums;

namespace OrderSystem.Modules.AI.DTOs;

public class ExplainOrderResponse
{
    public Guid OrderId { get; set; }

    public OrderStatus CurrentStatus { get; set; }

    public string Answer { get; set; } = string.Empty;

    public IReadOnlyList<KnowledgeSourceResponse> Sources { get; set; } =
        new List<KnowledgeSourceResponse>();
}
