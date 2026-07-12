using FluentValidation;
using ETicaret.Business.DTOs;

namespace ETicaret.Business.Validators;

public class AddressCreateValidator : AbstractValidator<AddressCreateDto>
{
    public AddressCreateValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad Soyad boş olamaz.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon numarası boş olamaz.")
            .Matches(@"^[0-9]{10,11}$").WithMessage("Geçerli bir telefon numarası giriniz.");

        RuleFor(x => x.CityId)
            .GreaterThan(0).WithMessage("Şehir seçilmelidir.");

        RuleFor(x => x.DistrictId)
            .GreaterThan(0).WithMessage("İlçe seçilmelidir.");

        RuleFor(x => x.DetailedAddress)
            .NotEmpty().WithMessage("Açık adres boş olamaz.")
            .Length(10, 500).WithMessage("Açık adres 10 ile 500 karakter arasında olmalıdır.");
    }
}
