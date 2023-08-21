using FluentValidation;
using Microsoft.Extensions.Options;
using SubtitleFileCleanerWeb.Api.Options;

namespace SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests.Validators;

public class CreateFromConversionValidator : AbstractValidator<CreateFromConversionRequest>
{
    public CreateFromConversionValidator(IOptions<FormFileOptions> options)
    {
        RuleFor(r => r.File).NotNull().WithMessage("The file must be provided for the request.");

        RuleFor(r => r.File).ChildRules((formFile) =>
        {
            formFile.RuleFor(f => f.Length)
                .GreaterThan(0)
                .WithMessage("File size cannot be zero.")
                .LessThan(options.Value.MaxFileLength)
                .WithMessage($"File size cannot be more then {options.Value.MaxFileLength}.");

            formFile.RuleFor(f => f.FileName).NotEmpty().WithMessage("File name must not be empty.");
            
        }).When(r => r.File is not null);
    }
}
