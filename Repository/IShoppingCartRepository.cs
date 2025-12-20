using SOLFranceBackend.Models.Dto;

namespace SOLFranceBackend.Repository
{
    public interface IShoppingCartRepository
    {
        Task<ResponseDto> GetShoppingCart(string userId, CancellationToken cancellationToken = default);
        Task<ResponseDto> AddShoppingCart(CartHeaderDto cartHeaderDto, CancellationToken cancellationToken = default);
        Task<ResponseDto> DeleteShoppingCart(int cartDetailsId, CancellationToken cancellationToken = default);
        Task<ResponseDto> DeleteEntireShoppingCart(string userId, CancellationToken cancellationToken = default);
    }
}
