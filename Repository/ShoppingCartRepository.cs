using AutoMapper;
//using MassTransit;
using Microsoft.EntityFrameworkCore;
using SOLFranceBackend.Data;
using SOLFranceBackend.Models;
using SOLFranceBackend.Models.Dto;
using SOLFranceBackend.Service.IService;

namespace SOLFranceBackend.Repository
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly AppDbContext _shoppingCartDbContext;
        private IProductService _productService;
        private ResponseDto _response;
        private IMapper _mapper;
        public ShoppingCartRepository(AppDbContext shoppingCartDbContext, IProductService productService, IMapper mapper)
        {
            _shoppingCartDbContext = shoppingCartDbContext;
            _productService = productService;
            _response = new();
            _mapper = mapper;
        }

        public async Task<ResponseDto> AddShoppingCart(CartHeaderDto cartHeaderDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var cartFromDb = _shoppingCartDbContext.CartHeaders.Include(c => c.CartDetailsList).FirstOrDefault(x => x.UserId == cartHeaderDto.UserId);
                if (cartFromDb == null)
                {
                    //create header and details
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartHeaderDto);
                    _shoppingCartDbContext.CartHeaders.Add(cartHeader);
                }
                else
                {
                    //if header is not null
                    //check if details has same product
                    var dbItems = cartFromDb.CartDetailsList;

                    var cartItem = dbItems.FirstOrDefault(x => x.ProductId == cartHeaderDto.CartDetailsList.First().ProductId);

                    if (cartItem != null)
                    {
                        cartItem.Count = cartHeaderDto.CartDetailsList.First().Count;
                    }

                    else
                    {
                        var item = cartHeaderDto.CartDetailsList.First();
                        var cardDetail = new CartDetails
                        {
                            CartHeaderId = item.CartHeaderId,
                            ProductId = item.ProductId,
                            Count = item.Count
                        };

                        _shoppingCartDbContext.CartDetails.Add(cardDetail);
                    }
                }
                await _shoppingCartDbContext.SaveChangesAsync();

                _response.Result = cartHeaderDto;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }

        public async Task<ResponseDto> DeleteShoppingCart(int cartDetailsId, CancellationToken cancellationToken = default)
        {
            try
            {
                CartDetails cartDetails = _shoppingCartDbContext.CartDetails
                   .First(u => u.CartDetailsId == cartDetailsId);

                int totalCountofCartItem = _shoppingCartDbContext.CartDetails.Where(u => u.CartHeaderId == cartDetails.CartHeaderId).Count();
                _shoppingCartDbContext.CartDetails.Remove(cartDetails);
                if (totalCountofCartItem == 1)
                {
                    var cartHeaderToRemove = await _shoppingCartDbContext.CartHeaders
                       .FirstOrDefaultAsync(u => u.CartHeaderId == cartDetails.CartHeaderId);

                    _shoppingCartDbContext.CartHeaders.Remove(cartHeaderToRemove);
                }
                await _shoppingCartDbContext.SaveChangesAsync();

                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }

        public async Task<ResponseDto> GetShoppingCart(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var cart = _shoppingCartDbContext.CartHeaders.Include(c => c.CartDetailsList).First(x => x.UserId == userId);

                IEnumerable<Product> objList = _shoppingCartDbContext.Products.ToList();
                IEnumerable<ProductDto> productDtos = _mapper.Map<IEnumerable<ProductDto>>(objList);

                foreach (var item in cart.CartDetailsList)
                {
                    item.Product = productDtos.FirstOrDefault(u => u.ProductId == item.ProductId);
                    cart.CartTotal += (item.Count * item.Product.Price);
                }

                _response.Result = cart;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        public async Task<ResponseDto> DeleteEntireShoppingCart(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var carHeaderToRemove = _shoppingCartDbContext.CartHeaders.First(x => x.UserId == userId);

                _shoppingCartDbContext.CartHeaders.Remove(carHeaderToRemove);

                await _shoppingCartDbContext.SaveChangesAsync();

                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }

    }
}
