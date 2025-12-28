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
                var isEmptyCart = cartHeaderDto.CartDetailsList == null || !cartHeaderDto.CartDetailsList.Any();
                
                if (cartFromDb == null)
                {
                    if (isEmptyCart)
                    {
                        // No cart exists and trying to set empty cart - just return success with null
                        _response.Result = null;
                        _response.IsSuccess = true;
                        _response.Message = "Cart is empty";
                        return _response;
                    }
                    
                    //create header and details
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartHeaderDto);
                    _shoppingCartDbContext.CartHeaders.Add(cartHeader);
                    await _shoppingCartDbContext.SaveChangesAsync();
                    
                    // Reload to get the generated CartHeaderId
                    cartFromDb = _shoppingCartDbContext.CartHeaders.Include(c => c.CartDetailsList).FirstOrDefault(x => x.UserId == cartHeaderDto.UserId);
                }
                else
                {
                    // Update existing cart - process all items from the request
                    var dbItems = cartFromDb.CartDetailsList ?? new List<CartDetails>();
                    
                    if (isEmptyCart)
                    {
                        // Clear all items from cart
                        foreach (var itemToRemove in dbItems.ToList())
                        {
                            _shoppingCartDbContext.CartDetails.Remove(itemToRemove);
                        }
                        
                        // Remove the cart header as well since it's empty
                        _shoppingCartDbContext.CartHeaders.Remove(cartFromDb);
                        await _shoppingCartDbContext.SaveChangesAsync();
                        
                        _response.Result = null;
                        _response.IsSuccess = true;
                        _response.Message = "Cart cleared";
                        return _response;
                    }
                    else
                    {
                        // Remove items that are not in the new cart
                        var itemsToRemove = dbItems.Where(dbItem => 
                            !cartHeaderDto.CartDetailsList.Any(newItem => newItem.ProductId == dbItem.ProductId)).ToList();
                        
                        foreach (var itemToRemove in itemsToRemove)
                        {
                            _shoppingCartDbContext.CartDetails.Remove(itemToRemove);
                        }

                        // Update or add items
                        foreach (var newItem in cartHeaderDto.CartDetailsList)
                        {
                            var existingItem = dbItems.FirstOrDefault(x => x.ProductId == newItem.ProductId);
                            
                            if (existingItem != null)
                            {
                                // Update existing item
                                existingItem.Count = newItem.Count;
                            }
                            else
                            {
                                // Add new item
                                var cartDetail = new CartDetails
                                {
                                    CartHeaderId = cartFromDb.CartHeaderId,
                                    ProductId = newItem.ProductId,
                                    Count = newItem.Count
                                };
                                _shoppingCartDbContext.CartDetails.Add(cartDetail);
                            }
                        }
                        
                        await _shoppingCartDbContext.SaveChangesAsync();
                        
                        // Reload to get updated cart
                        cartFromDb = _shoppingCartDbContext.CartHeaders.Include(c => c.CartDetailsList).FirstOrDefault(x => x.UserId == cartHeaderDto.UserId);
                    }
                }

                // Map the saved cart back to DTO for response
                if (cartFromDb != null)
                {
                    var cartResponse = _mapper.Map<CartHeaderDto>(cartFromDb);
                    _response.Result = cartResponse;
                }
                else
                {
                    _response.Result = null;
                }
                _response.IsSuccess = true;
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
                var cart = _shoppingCartDbContext.CartHeaders.Include(c => c.CartDetailsList).FirstOrDefault(x => x.UserId == userId);
                
                if (cart == null)
                {
                    _response.Result = null;
                    _response.IsSuccess = true;
                    _response.Message = "Cart not found";
                    return _response;
                }

                IEnumerable<ProductDto> productDtos = await _productService.GetProducts();

                // Reset cart total before calculating
                cart.CartTotal = 0;

                if (cart.CartDetailsList != null)
                {
                    foreach (var item in cart.CartDetailsList)
                    {
                        item.Product = productDtos.FirstOrDefault(u => u.ProductId == item.ProductId);
                        if (item.Product != null)
                        {
                            cart.CartTotal += (item.Count * item.Product.Price);
                        }
                    }
                }

                _response.Result = cart;
                _response.IsSuccess = true;
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
