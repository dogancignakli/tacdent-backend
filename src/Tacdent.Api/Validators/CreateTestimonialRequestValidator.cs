using FluentValidation;
using Tacdent.Api.ViewModels;

namespace Tacdent.Api.Validators;

public class CreateTestimonialRequestValidator : AbstractValidator<CreateTestimonialRequest>
{
    public CreateTestimonialRequestValidator()
    {
        RuleFor(x => x.AuthorName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.QuoteTr).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.QuoteEn).MaximumLength(1000);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).When(x => x.Rating.HasValue);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateTestimonialRequestValidator : AbstractValidator<UpdateTestimonialRequest>
{
    public UpdateTestimonialRequestValidator()
    {
        RuleFor(x => x.AuthorName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.QuoteTr).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.QuoteEn).MaximumLength(1000);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).When(x => x.Rating.HasValue);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
