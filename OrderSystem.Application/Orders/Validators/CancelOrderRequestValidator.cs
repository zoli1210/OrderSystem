using FluentValidation;
using OrderSystem.Application.Orders.Contracts.Requests;

namespace OrderSystem.Application.Orders.Validators;

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
