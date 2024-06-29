using Microsoft.AspNetCore.Http;
using Moq;
using SimpleWebApplication.Helpers;
using SimpleWebApplication.WebFirewall;
using Xunit;

namespace UnitTesting;

public class UserAgentTesting
{
    private readonly Mock<HttpContext> _mockContext;
    private readonly Mock<HttpRequest> _mockRequest;
    private readonly Mock<HttpResponse> _mockResponse;
    private readonly AuditConfiguration _auditMock;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly MemoryStream _memoryStream;
    
    public UserAgentTesting()
    {
        _mockContext = new Mock<HttpContext>();
        _mockRequest = new Mock<HttpRequest>();
        _mockResponse = new Mock<HttpResponse>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _auditMock = new AuditConfiguration(_mockHttpContextAccessor.Object);
        _memoryStream = new MemoryStream();

        _mockRequest.SetupGet(r => r.Headers).Returns(new HeaderDictionary());
        _mockResponse.SetupGet(r => r.Body).Returns(_memoryStream);
        _mockContext.SetupGet(c => c.Request).Returns(_mockRequest.Object);
        _mockContext.SetupGet(c => c.Response).Returns(_mockResponse.Object);
    }
    
    [Theory]
    [InlineData("Mozilla/5.0", true)]
    [InlineData("BadBot/1.0", false)]
    [InlineData("", false)]
    public async Task CheckRequestAsync_UnitTests(string userAgent, bool expected)
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = _memoryStream;
        ctx.Request.Headers["User-Agent"] = userAgent;
        var security = new UserAgentFilteringSecurity(_auditMock);

        var result = await security.CheckRequestAsync(ctx);

        if (expected)
        {
            Assert.True(result);
            Assert.NotEqual(StatusCodes.Status403Forbidden, ctx.Response.StatusCode);
        }
        else
        {
            Assert.False(result);
            Assert.Equal(StatusCodes.Status403Forbidden, ctx.Response.StatusCode);

            _memoryStream.Seek(0, SeekOrigin.Begin);
            var responseText = new StreamReader(_memoryStream).ReadToEnd();
            Assert.Equal(Messages.DeniedBrowser, responseText);
        }
    }
}