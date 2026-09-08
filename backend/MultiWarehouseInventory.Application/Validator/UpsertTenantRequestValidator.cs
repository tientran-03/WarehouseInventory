using FluentValidation;
using MultiWarehouseInventory.Application.DTOs;


namespace MultiWarehouseInventory.Application.Validator
{
    public class UpsertTenantRequestValidator : AbstractValidator<UpsertTenantRequest>
    {
        public UpsertTenantRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên Tenant không được để trống.")
                .MaximumLength(200).WithMessage("Tên Tenant không được vượt quá 200 ký tự.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Mã công ty không được để trống.")
                .MaximumLength(50).WithMessage("Mã công ty không được vượt quá 50 ký tự.")
                .Matches("^[a-zA-Z0-9-_]+$").WithMessage("Mã công ty chỉ được chứa chữ cái, số, dấu gạch ngang hoặc gạch dưới.");

            RuleFor(x => x.AccountEmail)
                .NotEmpty().WithMessage("Email tài khoản công ty không được để trống.")
                .EmailAddress().WithMessage("Email tài khoản công ty không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.AccountPassword));

            RuleFor(x => x.AccountPassword)
                .MinimumLength(8).WithMessage("Mật khẩu tạm thời cần tối thiểu 8 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.AccountPassword));
        }
    }
}
