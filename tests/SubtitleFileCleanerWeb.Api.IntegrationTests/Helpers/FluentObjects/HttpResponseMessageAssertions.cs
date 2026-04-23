using AwesomeAssertions.Execution;
using AwesomeAssertions.Primitives;

namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.FluentObjects;

public class HttpResponseMessageAssertions : ObjectAssertions<HttpResponseMessage, HttpResponseMessageAssertions>
{
    public HttpResponseMessageAssertions(HttpResponseMessage responce, AssertionChain assertionChain)
        : base(responce, assertionChain) { }

    public AndConstraint<HttpResponseMessageAssertions> HaveStatusCode(HttpStatusCode statusCode)
    {
        Subject.StatusCode.Should().Be(statusCode);

        return new AndConstraint<HttpResponseMessageAssertions>(this);
    }

    public AndConstraint<HttpResponseMessageAssertions> HaveReasonPhrase(string phrase)
    {
        Subject.ReasonPhrase.Should().Be(phrase);

        return new AndConstraint<HttpResponseMessageAssertions>(this);
    }

    public AndConstraint<HttpResponseMessageAssertions> HaveContentType(string mediaType, string? charSet = null)
    {
        Subject.Content.Headers.ContentType.Should().NotBeNull();
        Subject.Content.Headers.ContentType.MediaType.Should().Be(mediaType);

        if (charSet is not null)
            Subject.Content.Headers.ContentType.CharSet.Should().Be(charSet);

        return new AndConstraint<HttpResponseMessageAssertions>(this);
    }

    public AndWhichConstraint<HttpResponseMessageAssertions, T> HaveNotNullBody<T>()
    {
        var body = Subject.Content.ReadFromJsonAsync<T>(default).Result;
        body.Should().NotBeNull();

        return new AndWhichConstraint<HttpResponseMessageAssertions, T>(this, body);
    }

    public AndWhichConstraint<HttpResponseMessageAssertions, byte[]> HaveBodyAsByteArray()
    {
        var byteArray = Subject.Content.ReadAsByteArrayAsync(default).Result;

        return new AndWhichConstraint<HttpResponseMessageAssertions, byte[]>(this, byteArray);
    }
}
