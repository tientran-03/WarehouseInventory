using FluentValidation;
using MultiWarehouseInventory.Application.DTOs;

namespace MultiWarehouseInventory.Application.Validator;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OrderCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Channel).NotEmpty().MaximumLength(32);
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.CustomerPhone).NotEmpty().MaximumLength(32);
        RuleFor(x => x.CustomerAddress).NotEmpty().MaximumLength(512);
        RuleFor(x => x.ShippingFee).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OrderItems).NotEmpty();
        RuleForEach(x => x.OrderItems).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
