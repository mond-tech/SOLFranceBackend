using Newtonsoft.Json;
using SOLFranceBackend.Models.Dto;
using SOLFranceBackend.Service.IService;
//using Steeltoe.Common.Discovery;
//using Steeltoe.Discovery;

namespace SOLFranceBackend.Service
{
    public class ProductDetailService : IProductDetailService
    {
        //private readonly DiscoveryHttpClientHandler _handler;

        public ProductDetailService(/*IDiscoveryClient discoveryClient*/)
        {
            //_handler = new DiscoveryHttpClientHandler(discoveryClient);
        }
        public async Task<IEnumerable<ProductDetailDto>> GetProductDetail()
        {
            //var client = new HttpClient(_handler, false);
            ////var response = await client.GetAsync("http://ProductDetailAPI/api/product");
            //var response = await client.GetAsync("https://localhost:7156/api/product");
            //var apiContent = await response.Content.ReadAsStringAsync();

            //var resp = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
            //if (resp.IsSuccess)
            //{
            //    return JsonConvert.DeserializeObject<IEnumerable<ProductDetailDto>>(Convert.ToString(resp.Result));
            //}
            return new List<ProductDetailDto>();
        }

        public async Task<IEnumerable<ProductDetailDto>> SaveProductDetail()
        {
            //var client = new HttpClient(_handler, false);
            ////var response = await client.GetAsync("http://ProductDetailAPI/api/product");
            //var response = await client.GetAsync("https://localhost:7156/api/product");
            //var apiContent = await response.Content.ReadAsStringAsync();

            //var resp = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
            //if (resp.IsSuccess)
            //{
            //    return JsonConvert.DeserializeObject<IEnumerable<ProductDetailDto>>(Convert.ToString(resp.Result));
            //}
            return new List<ProductDetailDto>();
        }
    }
}
