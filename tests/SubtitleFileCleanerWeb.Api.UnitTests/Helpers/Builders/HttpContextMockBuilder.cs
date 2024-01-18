namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Builders;

public class HttpContextMockBuilder
{
    private readonly Mock<HttpContext> _contextMock = new();

    public HttpContext Build()
    {
        return _contextMock.Object;
    }

    public HttpContextMockBuilder SetupIMediator(Mock<IMediator> mediatorMock)
    {
        _contextMock.Setup(c => c.RequestServices.GetService(typeof(IMediator)))
            .Returns(mediatorMock.Object);

        return this;
    }

    public HttpContextMockBuilder SetupIMapper(Mock<IMapper> mapperMock)
    {
        _contextMock.Setup(c => c.RequestServices.GetService(typeof(IMapper)))
            .Returns(mapperMock.Object);

        return this;
    }
}
