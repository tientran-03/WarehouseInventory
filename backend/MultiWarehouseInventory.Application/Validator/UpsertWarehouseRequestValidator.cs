using FluentValidation;
using MultiWarehouseInventory.Application.DTOs;

namespace MultiWarehouseInventory.Application.Validators
{
    public class UpsertWarehouseRequestValidator : AbstractValidator<UpsertWarehouseRequest>
    {
        public UpsertWarehouseRequestValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Mã kho không được để trống.")
                .MaximumLength(50).WithMessage("Mã kho không được vượt quá 50 ký tự.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên kho không được để trống.")
                .MaximumLength(200).WithMessage("Tên kho không được vượt quá 200 ký tự.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Địa chỉ kho không được để trống.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("Thành phố không được để trống.");
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90m, 90m).WithMessage("Vĩ độ (Latitude) phải nằm trong khoảng từ -90 đến 90.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180m, 180m).WithMessage("Kinh độ (Longitude) phải nằm trong khoảng từ -180 đến 180.");
        }
    }
}