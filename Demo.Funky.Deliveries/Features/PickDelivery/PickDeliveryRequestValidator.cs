using Demo.Funky.Deliveries.Shared;
using FluentValidation;

namespace Demo.Funky.Deliveries.Features.PickDelivery
{
    public class PickDeliveryRequestValidator : ModelValidatorBase<PickDeliveryRequest>
    {
        public PickDeliveryRequestValidator()
        {
            RuleFor(x => x.CorrelationId).NotNull().NotEmpty().WithMessage("correlationId is null or empty");
            RuleFor(x=>x.OrderId).NotNull().NotEmpty().WithMessage("orderId is null or empty");
            RuleFor(x => x.CustomerMobile).NotNull().NotEmpty().WithMessage("invalid mobile number");
        }
    }
}