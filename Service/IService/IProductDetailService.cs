using SOLFranceBackend.Models.Dto;

namespace SOLFranceBackend.Service.IService
{
    public interface IProductDetailService
    {
        Task<IEnumerable<ProductDetailDto>> GetProductDetail();

        Task<IEnumerable<ProductDetailDto>> SaveProductDetail();
    }
}
