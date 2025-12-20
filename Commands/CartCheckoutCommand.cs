using SOLFranceBackend.Events;
using MediatR;
using SOLFranceBackend.Models.Dto;

namespace SOLFranceBackend.Commands
{
    public class CartCheckoutCommand : IRequest<ResponseDto>
    {
        public CartCheckoutEvent CartCheckoutEventModel { get; set; }
    }
}
