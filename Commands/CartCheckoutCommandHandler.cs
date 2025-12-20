using AutoMapper;
using SOLFranceBackend.Events;
using FluentValidation;
//using MassTransit;
using MediatR;
using SOLFranceBackend.Commands;
using SOLFranceBackend.Models;
using SOLFranceBackend.Models.Dto;
using SOLFranceBackend.Repository;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SOLFranceBackend.CommandHandlers
{
    public class CheckoutBasketCommandValidator
        : AbstractValidator<CartCheckoutCommand>
    {
        public CheckoutBasketCommandValidator()
        {
            RuleFor(x => x.CartCheckoutEventModel).NotNull().WithMessage("CartCheckoutDto can't be null");
            RuleFor(x => x.CartCheckoutEventModel.UserId).NotEmpty().WithMessage("UserId is required");
        }
    }

    public class CartCheckoutCommandHandler
        : IRequestHandler<CartCheckoutCommand, ResponseDto>
    {
        private readonly IShoppingCartRepository _repository;
        //private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;
        private ResponseDto _responseDto;

        public CartCheckoutCommandHandler(IShoppingCartRepository repository, /*IPublishEndpoint publishEndpoint,*/ IMapper mapper)
        {
            _repository = repository;
            //_publishEndpoint = publishEndpoint;
            _mapper = mapper;
            _responseDto = new();
        }

        public async Task<ResponseDto> Handle(CartCheckoutCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var cart = await _repository.GetShoppingCart(request.CartCheckoutEventModel.UserId, cancellationToken);
                var isTrue = cart.Result != null && cart.Result is CartHeader;
                if (isTrue)
                {
                    var cartHeader = cart.Result as CartHeader;
                    var cartHeaderDto = _mapper.Map<SOLFranceBackend.Models.Dto.CartHeaderDto>(cartHeader);
                    var eventMessage = new CartCheckoutEvent();
                    eventMessage.TotalPrice = Convert.ToDecimal(cartHeaderDto?.CartDetailsList?.Sum(x => x.Count * x.Product?.Price));
                    eventMessage.CartHeaderDto = cartHeaderDto;
                    //await _publishEndpoint.Publish(eventMessage, cancellationToken);
                    await _repository.DeleteEntireShoppingCart(request.CartCheckoutEventModel.UserId, cancellationToken);
                    _responseDto.Result = cartHeaderDto;
                }
                else
                {
                    _responseDto.IsSuccess = false;
                    _responseDto.Message = "Checkout unsuccessful";
                }
                return _responseDto;
            }
            catch (Exception ex)
            {
                _responseDto.Message = ex.Message.ToString();
                _responseDto.IsSuccess = false;
                return _responseDto;
            }
        }
    }
}
