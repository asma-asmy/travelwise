using FluentValidation;
using TravelWise.Application.Features.Auth.Dtos;

namespace TravelWise.Application.Features.Auth.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}