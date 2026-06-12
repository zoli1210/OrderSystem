using FluentValidation;
using OrderSystem.Application.Orders.Contracts.Queries;

namespace OrderSystem.Modules.Orders.Validators;

public class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    private static readonly string[] AllowedSortFields =
    [
        "createdAtUtc",
        "totalAmount",
        "status",
        "customerName",
    ];

    private static readonly string[] AllowedSortOrders = ["asc", "desc"];

    public GetOrdersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.SortBy)
            .Must(sortBy => AllowedSortFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            .WithMessage("SortBy must be one of: createdAtUtc, totalAmount, status, customerName.");

        RuleFor(x => x.SortOrder)
            .Must(sortOrder =>
                AllowedSortOrders.Contains(sortOrder, StringComparer.OrdinalIgnoreCase)
            )
            .WithMessage("SortOrder must be either asc or desc.");
    }
}
