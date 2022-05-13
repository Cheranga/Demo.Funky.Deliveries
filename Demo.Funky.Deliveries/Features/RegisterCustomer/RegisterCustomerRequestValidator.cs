using Demo.Funky.Deliveries.Shared;
using FluentValidation;

namespace Demo.Funky.Deliveries.Features.RegisterCustomer
{
    public class RegisterCustomerRequestValidator : ModelValidatorBase<RegisterCustomerRequest>
    {
        public RegisterCustomerRequestValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("name cannot be null or empty");
            RuleFor(x=>x.Address).NotNull().NotEmpty().WithMessage("address cannot be null or empty");
            // TODO: perform the other validations
        }
    }
}