using FluentValidation;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Domain.Validators.FileContextValidators;

public class FileContextValidator: AbstractValidator<FileContext>
{
    public FileContextValidator()
    {
        RuleFor(fc => fc.Name).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("File context name cannot be null.")
            .NotEmpty().WithMessage("File context name cannot be empty.");

        RuleFor(fc => fc.ContentSize)
            .GreaterThan(0).WithMessage("File context content size must be greater then 0.");
    }
}
