using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Mvc;
using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Wrappers;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Controllers.V1
{
    public class TestBaseController
    {
        private readonly BaseControllerWrapper _baseControllerWrapper;

        public TestBaseController()
        {
            _baseControllerWrapper = new BaseControllerWrapper();
        }

        [Fact]
        public void HandleErrorResponse_WithUnknownError_ReturnBadRequestResult()
        {
            // Arrange
            var errorMessage = "Test unknown error occurred.";
            var errors = new List<Error>
            {
                new Error { Code = ErrorCode.UnknownError, Message = errorMessage }
            };

            // Act
            var result = _baseControllerWrapper.HandleErrorResponse(errors);

            // Assert
            result.Should().NotBeNull()
                .And.BeOfType<BadRequestObjectResult>()

                .Which.Should().HaveStatusCode(400)
                .And.HaveNotNullValue()
                .And.HaveValueOfType<ErrorResponse>()

                .Which.Should().HaveStatusCode(400)
                .And.HaveStatusPhrase("Bad Request")
                .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Minutes())
                .And.HaveSingleError(errorMessage);
        }

        [Fact]
        public void HandleErrorResponse_WithTwoUnknownErrors_ReturnBadRequestResult()
        {
            // Arrange
            var firstErrorMessage = "First test unknow error occurred.";
            var secondErrorMessage = "Second test unknown error occurred.";

            var errors = new List<Error>
            {
                new Error { Code = ErrorCode.UnknownError, Message = firstErrorMessage },
                new Error { Code = ErrorCode.UnknownError, Message = secondErrorMessage }
            };

            // Act
            var result = _baseControllerWrapper.HandleErrorResponse(errors);

            // Assert
            result.Should().NotBeNull()
                .And.BeOfType<BadRequestObjectResult>()

                .Which.Should().HaveStatusCode(400)
                .And.HaveNotNullValue()
                .And.HaveValueOfType<ErrorResponse>()

                .Which.Should().HaveStatusCode(400)
                .And.HaveStatusPhrase("Bad Request")
                .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Minutes())
                .And.HaveErrors(firstErrorMessage, secondErrorMessage);

        }

        [Fact]
        public void HandleErrorResponse_WithNotFoundError_ReturnNotFoundResult()
        {
            // Arrange
            var errorMessage = "Test not found error occurred.";
            var errors = new List<Error>
            {
                new Error { Code = ErrorCode.NotFound, Message = errorMessage }
            };

            // Act
            var result = _baseControllerWrapper.HandleErrorResponse(errors);

            // Assert
            result.Should().NotBeNull()
                .And.BeOfType<NotFoundObjectResult>()

                .Which.Should().HaveStatusCode(404)
                .And.HaveNotNullValue()
                .And.HaveValueOfType<ErrorResponse>()

                .Which.Should().HaveStatusCode(404)
                .And.HaveStatusPhrase("Not Found")
                .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Minutes())
                .And.HaveSingleError(errorMessage);
        }

        [Fact]
        public void HandleErrorResponse_WithUnprocessableContentError_ReturnUnprocessableEntityResult()
        {
            // Arrange
            var errorMessage = "Test unprocessable content error occurred";
            var errors = new List<Error> 
            {
                new Error { Code = ErrorCode.UnprocessableContent, Message = errorMessage }
            };

            // Act
            var result = _baseControllerWrapper.HandleErrorResponse(errors);

            // Assert
            result.Should().NotBeNull()
                .And.BeOfType<UnprocessableEntityObjectResult>()

                .Which.Should().HaveStatusCode(422)
                .And.HaveNotNullValue()
                .And.HaveValueOfType<ErrorResponse>()

                .Which.Should().HaveStatusCode(422)
                .And.HaveStatusPhrase("Unprocessable Content")
                .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Minutes())
                .And.HaveSingleError(errorMessage);
        }
    }
}
