﻿using SubtitleFileCleanerWeb.Domain.Exceptions;
using SubtitleFileCleanerWeb.Domain.Validators.FileContextValidators;

namespace SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate
{
    public class FileContent
    {
        public Stream Content { get; private set; } = null!;

        private FileContent() { }

        /// <summary>
        /// Creates a new file content instance
        /// </summary>
        /// <param name="content">File content stream</param>
        /// <returns cref="FileContent"></returns>
        /// <exception cref="FileContentNotValidException"></exception>
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
