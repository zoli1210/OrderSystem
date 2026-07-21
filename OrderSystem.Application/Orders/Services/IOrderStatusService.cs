using OrderSystem.Application.Orders.Contracts.Requests;
using OrderSystem.Application.Orders.Contracts.Responses;

namespace OrderSystem.Application.Orders.Services;

public interface IOrderStatusService
{
    Task<UpdateOrderStatusResponse> UpdateStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequest request,
        string currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken
    );
}
