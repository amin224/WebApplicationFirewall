using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Moq;
using SimpleWebApplication.Helpers;
using SimpleWebApplication.WebFirewall;
using Xunit;

namespace UnitTesting;

public class DDoSTesting
{
    private readonly AuditConfiguration _auditMock;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly MemoryStream _memoryStream;

    private static readonly ConcurrentDictionary<string, ClientRequest> ClientRequest = new();

    public DDoSTesting()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _auditMock = new AuditConfiguration(_mockHttpContextAccessor.Object);
        _memoryStream = new MemoryStream();
    }

    [Theory]
    [InlineData(9, true)] // Within limit
    [InlineData(10, true)] // At the limit
    public async Task CheckRequestAsync_RequestsWithinLimit_ReturnsTrue(int requestCount, bool expected)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = _memoryStream;
        context.Connection.RemoteIpAddress = new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 });

        var auditMock = new Mock<AuditConfiguration>(_mockHttpContextAccessor.Object);
        var ddosSecurity = new DDoSSecurity(auditMock.Object);

        ClientRequest.AddOrUpdate(context.Connection.RemoteIpAddress.ToString(),
            new ClientRequest(requestCount, DateTime.UtcNow, 0), (_, _) => new ClientRequest(requestCount, DateTime.UtcNow, 0));

        var result = await ddosSecurity.CheckRequestAsync(context);

        Assert.Equal(expected, result);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }
}