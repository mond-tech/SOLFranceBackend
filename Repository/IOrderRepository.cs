using SOLFranceBackend.Models.Dto;

namespace SOLFranceBackend.Repository
{
    public interface IOrderRepository
    {
        ResponseDto GetOrdersByUserId(string userRole, string userId, CancellationToken cancellationToken = default);
        ResponseDto GetOrdersByOrderId(int orderId, CancellationToken cancellationToken = default);
        Task<ResponseDto> CreateOrder(CartHeaderDto cartHeaderDto, CancellationToken cancellationToken = default);
        ResponseDto UpdateOrderStatus(int orderId, string newStatus, CancellationToken cancellationToken = default);
    }
}
