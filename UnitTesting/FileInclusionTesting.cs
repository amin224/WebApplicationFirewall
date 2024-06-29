using System.Text;
using Microsoft.AspNetCore.Http;
using Moq;
using SimpleWebApplication.Helpers;
using SimpleWebApplication.WebFirewall;
using Xunit;

namespace UnitTesting;

public class FileInclusionTesting
{
    private readonly Mock<HttpContext> _mockContext;
    private readonly Mock<HttpRequest> _mockRequest;
    private readonly Mock<HttpResponse> _mockResponse;
    private readonly Mock<IFormFile> _mockFormFile;
    private readonly MemoryStream _memoryStream;
    private readonly AuditConfiguration _auditMock;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    
    public FileInclusionTesting()
    {
        _mockContext = new Mock<HttpContext>();
        _mockRequest = new Mock<HttpRequest>();
        _mockResponse = new Mock<HttpResponse>();
        _mockFormFile = new Mock<IFormFile>();
        _memoryStream = new MemoryStream();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _auditMock = new AuditConfiguration(_mockHttpContextAccessor.Object);

        _mockResponse.SetupGet(r => r.Body).Returns(_memoryStream);
        _mockContext.SetupGet(c => c.Request).Returns(_mockRequest.Object);
        _mockContext.SetupGet(c => c.Response).Returns(_mockResponse.Object);
    }
    
    [Fact]
    public async Task CheckRequestAsync_NoFileInput()
    {
        _mockRequest.Setup(r => r.Form.Files.Count).Returns(0);
        var security = new FileInclusionSecurity(_auditMock);
        var result = await security.CheckRequestAsync(_mockContext.Object);
        
        Assert.True(result);
    }
    
    [Fact]
    public async Task CheckRequestAsync_FileInclusion()
    {
        const string text = "<script>alert('xss');</script>";
        const string fileName = "malicious.txt";

        SetupFormFile(text, fileName);
        _mockRequest.Setup(r => r.Form.Files).Returns(new FormFileCollection { _mockFormFile.Object });
        var security = new FileInclusionSecurity(_auditMock);
        
        var result = await security.CheckRequestAsync(_mockContext.Object);
        
        Assert.True(result);
    }
    
    [Fact]
    public async Task CheckRequestAsync_NoFileInclusion()
    {
        const string text = "This is a safe file content.";
        const string fileName = "safe.txt";

        SetupFormFile(text, fileName);
        _mockRequest.Setup(r => r.Form.Files).Returns(new FormFileCollection { _mockFormFile.Object });
        var security = new FileInclusionSecurity(_auditMock);
        
        var result = await security.CheckRequestAsync(_mockContext.Object);
        
        Assert.True(result);
    }
    
    [Theory]
    [InlineData("http://malicious.com/unsafe.txt", false)]
    public async Task SafeRemoteFileInclude_UnitTests(string url, bool expected)
    {
        var security = new FileInclusionSecurity(_auditMock);
        
        var result = await security.SafeRemoteFileInclude(url);
        
        Assert.Equal(expected, result);
    }
    
    private void SetupFormFile(string content, string fileName)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        _mockFormFile.Setup(f => f.OpenReadStream()).Returns(stream);
        _mockFormFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Callback<Stream, CancellationToken>((s, t) => stream.CopyTo(s));
        _mockFormFile.Setup(f => f.FileName).Returns(fileName);
        _mockFormFile.Setup(f => f.Length).Returns(stream.Length);
    }
}