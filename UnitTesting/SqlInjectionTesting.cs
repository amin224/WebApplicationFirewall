using Microsoft.AspNetCore.Http;
using Moq;
using SimpleWebApplication.Helpers;
using SimpleWebApplication.WebFirewall;
using Xunit;

namespace UnitTesting;


public class SqlInjectionTesting
{
    private readonly Mock<HttpContext> _mockContext;
    private readonly Mock<HttpRequest> _mockRequest;
    private readonly Mock<HttpResponse> _mockResponse;
    private readonly MemoryStream _memoryStream;
    private readonly AuditConfiguration _auditMock;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

    public SqlInjectionTesting()
    {
        _mockContext = new Mock<HttpContext>(); 
        _mockRequest = new Mock<HttpRequest>();
        _mockResponse = new Mock<HttpResponse>();
        _memoryStream = new MemoryStream();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _auditMock = new AuditConfiguration(_mockHttpContextAccessor.Object);
        
        _mockResponse.SetupGet(r => r.Body).Returns(_memoryStream);
        _mockContext.SetupGet(c => c.Request).Returns(_mockRequest.Object);
        _mockContext.SetupGet(c => c.Response).Returns(_mockResponse.Object);
    }
    
    [Fact]
    public async Task CheckRequestAsync_NoQueryInput()
    {
        _mockRequest.Setup(r => r.QueryString).Returns(QueryString.Empty);
        var security = new SqlInjectionSecurity(_auditMock);
        
        var result = await security.CheckRequestAsync(_mockContext.Object);
        
        Assert.True(result);
    }
    
    [Theory]
    [InlineData("?input=SELECT * FROM users")]
    [InlineData("?input=' OR '1'='1")]
    [InlineData("?input=;DROP TABLE users--")]
    public async Task CheckRequestAsync_SqlInjection(string queryString)
    {
        var ctx = new DefaultHttpContext
        {
            Request =
            {
                QueryString = new QueryString(queryString)
            }
        };
        var memoryStream = new MemoryStream();
        ctx.Response.Body = memoryStream;

        var security = new SqlInjectionSecurity(_auditMock);
        var result = await security.CheckRequestAsync(ctx);
        
        Assert.False(result);
        Assert.Equal(StatusCodes.Status400BadRequest, ctx.Response.StatusCode);

        memoryStream.Seek(0, SeekOrigin.Begin);
        var responseText = new StreamReader(memoryStream).ReadToEnd();
        Assert.Equal("Sql injection attack detected.", responseText);
    }
    
    [Theory]
    [InlineData("?input=normalinput")]
    [InlineData("?input=safequery")]
    public async Task CheckRequestAsync_NoSqlInjection(string queryString)
    {
        _mockRequest.Setup(r => r.QueryString).Returns(new QueryString(queryString));
        _mockResponse.SetupProperty(r => r.StatusCode, StatusCodes.Status200OK);

        var security = new SqlInjectionSecurity(_auditMock);
        var result = await security.CheckRequestAsync(_mockContext.Object);
        
        Assert.True(result);
        Assert.Equal(StatusCodes.Status200OK, _mockContext.Object.Response.StatusCode);
    }
}