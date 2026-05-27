using OrderSystem.Modules.AI.DTOs;

namespace OrderSystem.Modules.AI.Services.OrderExplanation;

public interface IOrderExplanationService
{
    Task<ExplainOrderResponse> ExplainAsync(
        Guid orderId,
        ExplainOrderRequest request,
        CancellationToken cancellationToken
    );
}
