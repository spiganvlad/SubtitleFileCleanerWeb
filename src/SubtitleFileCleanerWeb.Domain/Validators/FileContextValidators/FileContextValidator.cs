using FluentValidation;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Domain.Validators.FileContextValidators;

public class FileContextValidator: AbstractValidator<FileContext>
{
    public FileContextValidator()
    {
        RuleFor(fc => fc.Name).Cascade(CascadeMode.Stop).
            NotNull().WithMessage("File context name cannot be null.").
            NotEmpty().WithMessage("File context name cannot be empty.");
    }
}
