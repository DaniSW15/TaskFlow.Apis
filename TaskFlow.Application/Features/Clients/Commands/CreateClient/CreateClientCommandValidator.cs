using FluentValidation;

namespace TaskFlow.Application.Features.Clients.Commands.CreateClient;

public sealed class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Client name is required.")
            .MaximumLength(200).WithMessage("Client name must not exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Client email is required.")
            .EmailAddress().WithMessage("Client email must be a valid email address.")
            .MaximumLength(256).WithMessage("Client email must not exceed 256 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("Phone must not exceed 30 characters.")
            .When(x => x.Phone is not null);

        RuleFor(x => x.Company)
            .MaximumLength(200).WithMessage("Company must not exceed 200 characters.")
            .When(x => x.Company is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.")
            .When(x => x.Notes is not null);
    }
}
