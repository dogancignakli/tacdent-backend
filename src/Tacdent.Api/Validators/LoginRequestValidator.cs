using FluentValidation;
using Tacdent.Api.ViewModels;

namespace Tacdent.Api.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Password).NotEmpty();
    }
}
