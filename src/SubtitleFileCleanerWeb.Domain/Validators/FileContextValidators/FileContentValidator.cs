using FluentValidation;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Domain.Validators.FileContextValidators;

public class FileContentValidator : AbstractValidator<FileContent>
{
    public FileContentValidator()
    {
        RuleFor(
            fileContent => fileContent.Content)
            .NotNull()
            .WithMessage("File content stream cannot be null.")
            .ChildRules(inlineValidator =>
            {
                inlineValidator.RuleFor(
                    content => content.Length)
                .GreaterThan(0)
                .WithMessage("File content stream cannot be empty.");

                inlineValidator.RuleFor(
                    content => content.CanWrite)
                .Must(canWrite => !canWrite)
                .WithMessage("File content stream must be readonly.");
            });
    }
}
