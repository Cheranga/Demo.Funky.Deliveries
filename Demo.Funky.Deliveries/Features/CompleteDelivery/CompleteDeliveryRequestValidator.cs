using Demo.Funky.Deliveries.Shared;
using FluentValidation;

namespace Demo.Funky.Deliveries.Features.CompleteDelivery
{
    public class CompleteDeliveryRequestValidator : ModelValidatorBase<CompleteDeliveryRequest>
    {
        public CompleteDeliveryRequestValidator()
        {
            RuleFor(x => x.CorrelationId).NotNull().NotEmpty().WithMessage("correlationId is required");
            RuleFor(x=>x.CustomerMobile).NotNull().NotEmpty().WithMessage("customer mobile number is required");
            RuleFor(x=>x.PickerMobile).NotNull().NotEmpty().WithMessage("picker mobile number is required");
            RuleFor(x=>x.OrderId).NotNull().NotEmpty().WithMessage("orderId is required");
        }
    }
}