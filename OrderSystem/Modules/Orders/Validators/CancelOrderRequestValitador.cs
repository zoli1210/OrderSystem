using FluentValidation;
using OrderSystem.Modules.Orders.DTOs;

namespace OrderSystem.Modules.Orders.Validators;

public class CancelOrderRequestValidator : AbstractValidator<CancelOrderRequest>
{
    public CancelOrderRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Cancellation reason is required.")
            .MaximumLength(500)
            .WithMessage("Cancellation reason must be maximum 500 characters.");
    }
}
