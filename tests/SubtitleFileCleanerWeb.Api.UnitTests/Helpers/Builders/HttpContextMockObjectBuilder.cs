namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Builders;

public class HttpContextMockObjectBuilder
{
    private readonly Mock<HttpContext> _contextMock = new();

    public HttpContext Build()
    {
        return _contextMock.Object;
    }

    public HttpContextMockObjectBuilder SetupIMediator(Mock<IMediator> mediatorMock)
    {
        _contextMock.Setup(c => c.RequestServices.GetService(typeof(IMediator)))
            .Returns(mediatorMock.Object);

        return this;
    }

    public HttpContextMockObjectBuilder SetupIMapper(Mock<IMapper> mapperMock)
    {
        _contextMock.Setup(c => c.RequestServices.GetService(typeof(IMapper)))
            .Returns(mapperMock.Object);

        return this;
    }
}
