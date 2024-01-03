using FluentValidation;
using Microsoft.Extensions.Options;
using SubtitleFileCleanerWeb.Api.Options;

namespace SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests.Validators;

public class CreateFromConversionValidator : AbstractValidator<CreateFromConversionRequest>
{
    public CreateFromConversionValidator(IOptions<FormFileOptions> options)
    {
        RuleFor(
            request => request.File)
            .NotNull()
            .WithMessage("The file must be provided for the request.")
            .ChildRules(inlineValidator =>
            {
                inlineValidator.RuleFor(
                    formFile => formFile.Length)
                .GreaterThan(0)
                .WithMessage("File size cannot be zero.")
                .LessThan(options.Value.MaxFileLength)
                .WithMessage($"File size cannot be more then {options.Value.MaxFileLength}.");

                inlineValidator.RuleFor(
                    formFile => formFile.FileName)
                .NotEmpty()
                .WithMessage("File name must not be empty.");
            });
    }
}
