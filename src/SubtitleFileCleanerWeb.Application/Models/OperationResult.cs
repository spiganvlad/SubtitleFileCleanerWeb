using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.Models
{
    public class OperationResult<T>
    {
        public T? Payload { get; set; }
        public bool IsError { get; private set; }
        public List<Error> Errors { get; } = [];

        /// <summary>
        /// Adds an error to the list of errors and changes the state of the result to error
        /// </summary>
        /// <param name="code">Error code</param>
        /// <param name="message">Error message</param>
        public void AddError(ErrorCode code, string message)
        {
            Errors.Add(new Error { Code = code, Message = message });
            IsError = true;
        }

        /// <summary>
        /// Adds an error to the list of errors with the error code UnknownError
        /// </summary>
        /// <param name="message">Error message</param>
        public void AddUnknownError(string message)
        {
            AddError(ErrorCode.UnknownError, message);
        }

        /// <summary>
        /// Copies errors to a list of errors and changes the state of the result to error
        /// </summary>
        /// <param name="errors">Enumeration of errors</param>
        public void CopyErrors(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
                Errors.Add(error);
            
            IsError = true;
        }
    }
}
