using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SIMS.Tests;

/// <summary>
/// Kiểm thử đầu cuối (End-to-End, E2E):
/// - Gửi HTTP request thật tới ứng dụng (chạy trong memory).
/// - Đi qua pipeline middleware, controller, service, repository, database.
/// - Ở đây tập trung vào luồng đăng nhập.
/// </summary>
public class LoginE2ETests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LoginE2ETests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Get_LoginIndex_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/Login/Index");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Login", html, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Post_Login_InvalidCredentials_ShowsErrorMessage()
    {
        // Arrange
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Username", "wrong"),
            new KeyValuePair<string, string>("Password", "invalid")
        });

        // Act
        var response = await _client.PostAsync("/Login/Index", content);

        // Assert: vẫn ở trang login, không redirect
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Your Account Invalid", html);
    }

    [Fact]
    public async Task Post_Login_ValidStudent_RedirectsToStudentDashboard()
    {
        // Arrange – dùng tài khoản đã seed trong CustomWebApplicationFactory
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Username", "S999"),
            new KeyValuePair<string, string>("Password", "password")
        });

        // Act
        var response = await _client.PostAsync("/Login/Index", content);

        // Assert: redirect tới StudentDashboard (có thể là /StudentDashboard hoặc /StudentDashboard/Index tuỳ routing)
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var location = response.Headers.Location!.ToString();
        Assert.Contains("/StudentDashboard", location);
    }
}


