using FluentValidation;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Domain.Validators.FileContextValidators;

public class FileContentValidator : AbstractValidator<FileContent>
{
    public FileContentValidator()
    {
        RuleFor(fc => fc.Content).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("File content stream cannot be null.")
            .Must(fc => fc.Length > 0).WithMessage("File content stream cannot be empty.")
            .Must(fc => !fc.CanWrite).WithMessage("File content stream must be readonly.");
    }
}
