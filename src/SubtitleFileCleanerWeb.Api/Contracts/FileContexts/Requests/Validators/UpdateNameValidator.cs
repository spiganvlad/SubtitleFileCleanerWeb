using FluentValidation;

namespace SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests.Validators;

public class UpdateNameValidator : AbstractValidator<UpdateNameRequest>
{
    public UpdateNameValidator()
    {
        RuleFor(r => r.Name).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("The update name must be provided for the request.")
            .NotEmpty().WithMessage("Update name must not be empty.");
    }
}
