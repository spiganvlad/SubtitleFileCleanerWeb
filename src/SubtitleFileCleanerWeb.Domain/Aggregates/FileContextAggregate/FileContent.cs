using SubtitleFileCleanerWeb.Domain.Exceptions;
using SubtitleFileCleanerWeb.Domain.Validators.FileContextValidators;

namespace SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate
{
    public class FileContent
    {
        public Stream Content { get; private set; } = null!;

        private FileContent() { }

        public static FileContent Create(Stream content)
        {
            var validator = new FileContentValidator();

            var fileContent = new FileContent { Content = content };
            
            var validationResult = validator.Validate(fileContent);
            if (validationResult.IsValid)
                return fileContent;

            var exception = new FileContentNotValidException("File content not valid.");
            validationResult.Errors.ForEach(e => exception.ValidationErrors.Add(e.ErrorMessage));
            throw exception;
        }
    }
}
