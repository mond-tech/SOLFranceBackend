using AutoMapper;
using SOLFranceBackend.Data;
using SOLFranceBackend.Models;
using SOLFranceBackend.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOLFranceBackend.Service.IService;
using SOLFranceBackend.Repository;

namespace SOLFranceBackend.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        public OrderAPIController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [Authorize]
        [HttpGet("GetOrders")]
        public ResponseDto? Get(string? userId = "")
        {
            if (User.IsInRole("ADMIN"))
            {
                return _orderRepository.GetOrdersByUserId("ADMIN", userId);
            }
            else
            {
                return _orderRepository.GetOrdersByUserId("CUSTOMER", userId);
            }
        }

        [Authorize]
        [HttpGet("GetOrder/{id:int}")]
        public ResponseDto? Get(int id)
        {
            return _orderRepository.GetOrdersByOrderId(id);
        }

        [HttpPost("CreateOrder")]
        public async Task<ResponseDto> CreateOrder([FromBody] CartHeaderDto cartHeaderDto)
        {
            return await _orderRepository.CreateOrder(cartHeaderDto);
        }

        [Authorize]
        [HttpPost("UpdateOrderStatus/{orderId:int}")]
        public ResponseDto UpdateOrderStatus(int orderId, [FromBody] string newStatus)
        {
            return _orderRepository.UpdateOrderStatus(orderId, newStatus);
        }
    }
}
