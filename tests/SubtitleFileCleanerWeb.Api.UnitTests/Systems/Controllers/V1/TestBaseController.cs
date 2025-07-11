using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Wrappers;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Controllers.V1
{
    public class TestBaseController
    {
        private readonly BaseControllerWrapper _sut = new();

        [Theory, AutoData]
        public void HandleErrorResponse_WithUnknownError_ReturnBadRequestResult
            (string message)
        {
            // Arrange
            List<Error> errors = [new() { Code = ErrorCode.UnknownError, Message = message }];

            // Act
            var result = _sut.HandleErrorResponse(errors);

            // Assert
            result.Should().NotBeNull()
                .And.BeOfType<BadRequestObjectResult>()

                .Which.Should().HaveStatusCode(400)
                .And.HaveNotNullValue()
                .And.HaveValueOfType<ErrorResponse>()

                .Which.Should().HaveStatusCode(400)
                .And.HaveStatusPhrase("Bad Request")
                .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Seconds())
                .And.HaveSingleError(message);
        }

        [Theory, AutoData]
        public void HandleErrorResponse_WithTwoUnknownErrors_ReturnBadRequestResult
            (string firstMessage, string secondMessage)
        {
            // Arrange
            List<Error> errors =
            [
                new() { Code = ErrorCode.UnknownError, Message = firstMessage },
                new() { Code = ErrorCode.UnknownError, Message = secondMessage }
            ];

            // Act
            var result = _sut.HandleErrorResponse(errors);

            // Assert
            result.Should().NotBeNull()
                .And.BeOfType<BadRequestObjectResult>()

                .Which.Should().HaveStatusCode(400)
                .And.HaveNotNullValue()
                .And.HaveValueOfType<ErrorResponse>()

                .Which.Should().HaveStatusCode(400)
                .And.HaveStatusPhrase("Bad Request")
                .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Seconds())
                .And.HaveErrors(firstMessage, secondMessage);

        }

        [Theory, AutoData]
        public void HandleErrorResponse_WithNotFoundError_ReturnNotFoundResult
            (string message)
        {
            // Arrange
            List<Error> errors = [new() { Code = ErrorCode.NotFound, Message = message }];

            // Act
            var result = _sut.HandleErrorResponse(errors);

            // Assert
            result.Should().NotBeNull()
                .And.BeOfType<NotFoundObjectResult>()

                .Which.Should().HaveStatusCode(404)
                .And.HaveNotNullValue()
                .And.HaveValueOfType<ErrorResponse>()

                .Which.Should().HaveStatusCode(404)
                .And.HaveStatusPhrase("Not Found")
                .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Seconds())
                .And.HaveSingleError(message);
        }

        [Theory, AutoData]
        public void HandleErrorResponse_WithUnprocessableContentError_ReturnUnprocessableEntityResult
            (string message)
        {
            // Arrange
            List<Error> errors = [new() { Code = ErrorCode.UnprocessableContent, Message = message }];

            // Act
            var result = _sut.HandleErrorResponse(errors);

            // Assert
            result.Should().NotBeNull()
                .And.BeOfType<UnprocessableEntityObjectResult>()

                .Which.Should().HaveStatusCode(422)
                .And.HaveNotNullValue()
                .And.HaveValueOfType<ErrorResponse>()

                .Which.Should().HaveStatusCode(422)
                .And.HaveStatusPhrase("Unprocessable Content")
                .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Seconds())
                .And.HaveSingleError(message);
        }
    }
}
