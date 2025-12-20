using SOLFranceBackend.Models.Dto;
using SOLFranceBackend.Service.IService;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using System.Text;
//using Steeltoe.Common.Discovery;
//using Steeltoe.Discovery;

namespace SOLFranceBackend.Service
{
    public class ProductService : IProductService
    {
        //private readonly DiscoveryHttpClientHandler _handler;

        public ProductService(/*IDiscoveryClient discoveryClient*/)
        {
            //_handler = new DiscoveryHttpClientHandler(discoveryClient);
        }
        public async Task<IEnumerable<ProductDto>> GetProducts()
        {
            //var client = new HttpClient(_handler, false);
            //var response = await client.GetAsync("http://ProductAPI/api/product");
            //var apiContent = await response.Content.ReadAsStringAsync();

            //var resp = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
            //if (resp.IsSuccess)
            //{
            //    return JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(Convert.ToString(resp.Result));
            //}
            return new List<ProductDto>();
        }
    }
}
