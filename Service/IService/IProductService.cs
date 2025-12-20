using SOLFranceBackend.Models.Dto;

namespace SOLFranceBackend.Service.IService
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProducts();
    }
}
