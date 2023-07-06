using FluentValidation;

namespace SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests.Validators;

public class FileContextCreateConvertedValidator : AbstractValidator<FileContextCreateConverted>
{
    public FileContextCreateConvertedValidator()
    {
        RuleFor(dto => dto.File).NotNull().WithMessage("The file must be provided for the request.");

        RuleFor(dto => dto.File).ChildRules((formFile) =>
        {
            formFile.RuleFor(f => f.Length).GreaterThan(0).WithMessage("File size cannot be zero.");
            formFile.RuleFor(f => f.FileName).NotEmpty().WithMessage("File name must not be empty.");
            
        }).When(dto => dto.File is not null);
    }
}
