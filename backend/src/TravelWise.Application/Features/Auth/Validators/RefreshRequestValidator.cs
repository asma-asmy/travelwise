using FluentValidation;
using TravelWise.Application.Features.Auth.Dtos;

namespace TravelWise.Application.Features.Auth.Validators;

public class RefreshRequestValidator : AbstractValidator<RefreshRequestDto>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}