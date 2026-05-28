using OrderSystem.Modules.Orders.DTOs;

namespace OrderSystem.Modules.Orders.Services;

public interface IOrderStatusService
{
    Task<UpdateOrderStatusResponse> UpdateStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken
    );
}
