using OrderSystem.Application.Orders.Contracts.Requests;
using OrderSystem.Application.Orders.Contracts.Responses;

namespace OrderSystem.Modules.Orders.Services;

public interface IOrderStatusService
{
    Task<UpdateOrderStatusResponse> UpdateStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken
    );
}
