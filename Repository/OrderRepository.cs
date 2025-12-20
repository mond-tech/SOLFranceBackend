using AutoMapper;
//using MassTransit;
using Microsoft.EntityFrameworkCore;
using SOLFranceBackend.Data;
using SOLFranceBackend.Models;
using SOLFranceBackend.Service.IService;
using SOLFranceBackend.Models.Dto;

namespace SOLFranceBackend.Repository
{
    public class OrderRepository : IOrderRepository
    {
        protected ResponseDto _response;
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;
        private readonly ILogger<OrderRepository> _logger;
        private readonly IConfiguration _configuration;

        public OrderRepository(IMapper mapper, AppDbContext db, ILogger<OrderRepository> logger, IConfiguration configuration)
        {
            _db = db;
            _response = new();
            _logger = logger;
            _mapper = mapper;
            _configuration = configuration;
        }
        public async Task<ResponseDto> CreateOrder(CartHeaderDto cartHeaderDto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Order creation starts");
                OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartHeaderDto);
                orderHeaderDto.OrderTime = DateTime.Now;
                orderHeaderDto.Status = "Pending";
                orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartHeaderDto.CartDetailsList);
                orderHeaderDto.OrderTotal = Math.Round(orderHeaderDto.OrderTotal, 2);
                OrderHeader orderCreated = _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;
                await _db.SaveChangesAsync();

                orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;
                _response.Result = orderHeaderDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                _logger.LogError("Issue in CreateOrder() " + ex.Message);
            }
            _logger.LogInformation("Order creation ends");
            return _response;
        }

        public ResponseDto GetOrdersByOrderId(int orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.Include(u => u.OrderDetails).First(u => u.OrderHeaderId == orderId);
                _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                _logger.LogError("Issue in GetOrdersByOrderId() " + ex.Message);
            }
            return _response;
        }

        public ResponseDto GetOrdersByUserId(string userRole, string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                IEnumerable<OrderHeader> objList;
                if (userRole == "ADMIN")
                {
                    objList = _db.OrderHeaders.Include(u => u.OrderDetails).OrderByDescending(u => u.OrderHeaderId).ToList();
                }
                else
                {
                    objList = _db.OrderHeaders.Include(u => u.OrderDetails).Where(u => u.UserId == userId).OrderByDescending(u => u.OrderHeaderId).ToList();
                }
                _response.Result = _mapper.Map<IEnumerable<OrderHeaderDto>>(objList);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                _logger.LogError("Issue in GetOrdersByUserId() " + ex.Message);
            }
            return _response;
        }

        public ResponseDto UpdateOrderStatus(int orderId, string newStatus, CancellationToken cancellationToken = default)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.First(u => u.OrderHeaderId == orderId);
                if (orderHeader != null)
                {
                    if (newStatus == "Cancelled")
                    {
                        //we will give refund, no code, just an use case
                    }
                    orderHeader.Status = newStatus;
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _logger.LogError("Issue in UpdateOrderStatus() " + ex.Message);
            }
            return _response;
        }
    }
}
