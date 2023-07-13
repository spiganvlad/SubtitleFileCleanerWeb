using SubtitleFileCleanerWeb.Domain.Exceptions.FileContextAggregateExceptions;
using SubtitleFileCleanerWeb.Domain.Validators.FileContextValidators;

namespace SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

public class FileContext
{
    public Guid FileContextId { get; private set; }
    public string Name { get; private set; } = null!;
    public FileContent? Content { get; private set; }
    public DateTime DateCreated { get; private set; }
    public DateTime DateModified { get; private set; }

    private FileContext() { }

    /// <summary>
    /// Creates a new file context instance
    /// </summary>
    /// <param name="name">File context name</param>
    /// <returns cref="FileContent"></returns>
    /// <exception cref="FileContextNotValidException"></exception>
    public static FileContext Create(string name)
    {
        var validator = new FileContextValidator();

        var fileContext = new FileContext
        {
            FileContextId = Guid.NewGuid(),
            Name = name,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };

        var validationResult = validator.Validate(fileContext);
        if (validationResult.IsValid)
            return fileContext;

        var exception = new FileContextNotValidException("File context is not valid.");
        validationResult.Errors.ForEach(e => exception.ValidationErrors.Add(e.ErrorMessage));
        throw exception;
    }

    /// <summary>
    /// Updates the file context name   
    /// </summary>
    /// <param name="name">The updated file context name</param>
    /// <exception cref="FileContextNotValidException"></exception>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            var exception = new FileContextNotValidException("Cannot update file context. " +
                "File context name is not valid.");
            exception.ValidationErrors.Add("The provided name is either null or white space.");
            throw exception;
        }

        Name = name;
        DateModified = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the file context content
    /// </summary>
    /// <param name="content">File content to set</param>
    /// <exception cref="FileContentAlreadySetException"></exception>
    public void SetContent(FileContent content)
    {
        if (Content is not null)
            throw new FileContentAlreadySetException("Cannot set file content. It is already set.");
        
        Content = content;
    }
}
