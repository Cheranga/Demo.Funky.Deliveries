using Demo.Funky.Deliveries.Shared;
using FluentValidation;

namespace Demo.Funky.Deliveries.Features.RegisterPicker
{
    public class RegisterPickerRequestValidator : ModelValidatorBase<RegisterPickerRequest>
    {
        public RegisterPickerRequestValidator()
        {
            RuleFor(x=>x.Name).NotNull().NotEmpty().WithMessage("name cannot be null or empty");
            // TODO: perform the other validations
        }
    }
}