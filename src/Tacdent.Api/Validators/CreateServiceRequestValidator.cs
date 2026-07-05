using FluentValidation;
using Tacdent.Api.ViewModels;

namespace Tacdent.Api.Validators;

public class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequest>
{
    public CreateServiceRequestValidator()
    {
        RuleFor(x => x.NameTr).NotEmpty().MaximumLength(120);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DescriptionTr).NotEmpty().MaximumLength(500);
        RuleFor(x => x.DescriptionEn).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Icon).MaximumLength(50);
        RuleFor(x => x.PriceFromTry).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PriceFromEur).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateServiceRequestValidator : AbstractValidator<UpdateServiceRequest>
{
    public UpdateServiceRequestValidator()
    {
        RuleFor(x => x.NameTr).NotEmpty().MaximumLength(120);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DescriptionTr).NotEmpty().MaximumLength(500);
        RuleFor(x => x.DescriptionEn).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Icon).MaximumLength(50);
        RuleFor(x => x.PriceFromTry).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PriceFromEur).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
