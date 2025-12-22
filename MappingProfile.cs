using AutoMapper;
using SOLFranceBackend.Models;
using SOLFranceBackend.Models.Dto;

namespace SOLFranceBackend
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<OrderHeaderDto, CartHeaderDto>()
                .ForMember(dest => dest.CartTotal,
                    opt => opt.MapFrom(src => src.OrderTotal))
                .ReverseMap();

            CreateMap<CartDetailsDto, OrderDetailsDto>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.Price,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Price : 0));

            CreateMap<OrderDetailsDto, CartDetailsDto>();
            CreateMap<OrderHeader, OrderHeaderDto>().ReverseMap();
            CreateMap<OrderDetailsDto, OrderDetails>().ReverseMap();
            CreateMap<ProductDto, Product>().ReverseMap();
            CreateMap<CartHeader, CartHeaderDto>().ReverseMap();
            CreateMap<CartDetails, CartDetailsDto>().ReverseMap();
            CreateMap<ApplicationUser, UserDto>().ReverseMap();
        }
    }

}
