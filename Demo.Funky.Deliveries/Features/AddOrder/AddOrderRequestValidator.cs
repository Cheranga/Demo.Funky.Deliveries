using Demo.Funky.Deliveries.Shared;
using FluentValidation;

namespace Demo.Funky.Deliveries.Features.AddOrder
{
    public class AddOrderRequestValidator : ModelValidatorBase<AddOrderRequest>
    {
        public AddOrderRequestValidator()
        {
            RuleFor(x => x.CustomerMobile).NotNull().NotEmpty().WithMessage("invalid mobile number");
            RuleFor(x => x.Price).GreaterThan(0);
            // TODO: perform the other validations
        }
    }
}