using AutoMapper;
using SOLFranceBackend.Models;
using SOLFranceBackend.Models.Dto;
using SOLFranceBackend.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOLFranceBackend.Data;
using SOLFranceBackend.Repository;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using Microsoft.AspNetCore.Hosting.Server;
using MediatR;
using SOLFranceBackend.Commands;
using SOLFranceBackend.Events;
//using MassTransit;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IMediator _mediator;
        private ResponseDto _responseDto;

        public CartAPIController(IShoppingCartRepository shoppingCartRepository, IMediator mediator)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _mediator = mediator;
            _responseDto = new();
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<string> GetCart(string userId)
        {
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                WriteIndented = true // Optional, for readable output
            };

            var response = await _shoppingCartRepository.GetShoppingCart(userId);

            var responseResult = response.Result as CartHeader;

            string json = JsonSerializer.Serialize(responseResult, options);

            return json;
        }

        [HttpPost("CartUpsert")]
        public async Task<ResponseDto> CartUpsert(CartHeaderDto cartHeaderDto)
        {
            return await _shoppingCartRepository.AddShoppingCart(cartHeaderDto);
        }

        [HttpPost("RemoveCart")]
        public async Task<ResponseDto> RemoveCart([FromBody] int cartDetailsId)
        {
            return await _shoppingCartRepository.DeleteShoppingCart(cartDetailsId);
        }

        [Authorize]
        [HttpPost("Checkout/{userId}")]
        public async Task<ResponseDto> Checkout(string userId)
        {
            try
            {
                var cartCheckoutEventModel = new CartCheckoutEvent()
                {
                    UserId = userId
                };

                _responseDto = await _mediator.Send(new CartCheckoutCommand()
                {
                    CartCheckoutEventModel = cartCheckoutEventModel
                });

            }
            catch(Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

    }
}
