using FluentValidation;
using OrderSystem.Modules.Orders.DTOs;

namespace OrderSystem.Modules.Orders.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().WithMessage("Customer name is required.");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .WithMessage("Customer email is required.")
            .EmailAddress()
            .WithMessage("Customer email is not valid.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .WithMessage("Total amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a valid 3-letter code.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Description must be maximum 500 characters.");
    }
}
