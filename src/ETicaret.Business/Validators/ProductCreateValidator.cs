using FluentValidation;
using ETicaret.Business.DTOs;

namespace ETicaret.Business.Validators;

public class ProductCreateValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Kategori seçilmelidir.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Ürün başlığı boş olamaz.")
            .Length(5, 200).WithMessage("Ürün başlığı 5 ile 200 karakter arasında olmalıdır.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Ürün açıklaması boş olamaz.")
            .Length(20, 5000).WithMessage("Ürün açıklaması 20 ile 5000 karakter arasında olmalıdır.");

        RuleFor(x => x.Price)
            .InclusiveBetween(0.01m, 1000000m).WithMessage("Fiyat 0.01 ile 1.000.000 arasında olmalıdır.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stok miktarı 0'dan küçük olamaz.");
    }
}
