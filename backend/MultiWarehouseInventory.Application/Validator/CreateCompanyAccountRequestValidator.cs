using FluentValidation;
using MultiWarehouseInventory.Application.DTOs;

namespace MultiWarehouseInventory.Application.Validator;

public class CreateCompanyAccountRequestValidator : AbstractValidator<CreateCompanyAccountRequest>
{
    public CreateCompanyAccountRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không hợp lệ.")
            .MaximumLength(254);
        RuleFor(x => x.Username)
            .MaximumLength(80)
            .When(x => !string.IsNullOrWhiteSpace(x.Username));
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(8);
    }
}
