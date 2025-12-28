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
        public async Task<ActionResult<ResponseDto>> GetCart(string userId)
        {
            var response = await _shoppingCartRepository.GetShoppingCart(userId);

            // response.Result CAN be null (valid case)
            return Ok(new ResponseDto
            {
                IsSuccess = true,
                Result = response.Result,
                Message = response.Result == null ? "Cart not found" : "Cart fetched"
            });
        }

        [HttpPost("CartUpsert")]
        public async Task<ActionResult<ResponseDto>> CartUpsert([FromForm] CartHeaderDto cartHeaderDto)
        {
            var response = await _shoppingCartRepository.AddShoppingCart(cartHeaderDto);
            return Ok(response);
        }

        [HttpPost("RemoveCart")]
        public async Task<ActionResult<ResponseDto>> RemoveCart([FromBody] int cartDetailsId)
        {
            var response = await _shoppingCartRepository.DeleteShoppingCart(cartDetailsId);
            return Ok(response);
        }


        [Authorize]
        [HttpPost("Checkout/{userId}")]
        public async Task<ActionResult<ResponseDto>> Checkout(string userId)
        {
            try
            {
                var cartCheckoutEventModel = new CartCheckoutEvent
                {
                    UserId = userId
                };

                var response = await _mediator.Send(new CartCheckoutCommand
                {
                    CartCheckoutEventModel = cartCheckoutEventModel
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

    }
}
