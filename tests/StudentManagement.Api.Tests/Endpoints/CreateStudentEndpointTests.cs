using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using StudentManagement.Api.Features.Students;
using StudentManagement.Api.Features.Students.CreateStudent;

namespace StudentManagement.Api.Tests.Endpoints;

[TestFixture]
public class CreateStudentEndpointTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private IStudentRepository _repository = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IStudentRepository>();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IStudentRepository>();
                services.AddSingleton(_repository);
            });
        });
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Post_WithValidRequest_Returns201WithCreatedStudent()
    {
        _repository.AddAsync(Arg.Any<Student>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Student>()));

        var request = new CreateStudentRequest("Ada", "Lovelace", new DateOnly(2005, 1, 1), "ada@example.com", "Grade 10");

        var response = await _client.PostAsJsonAsync("/api/students", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var body = await response.Content.ReadFromJsonAsync<StudentResponse>();
        Assert.That(body, Is.Not.Null);
        Assert.That(body!.FirstName, Is.EqualTo("Ada"));
    }

    [Test]
    public async Task Post_WithInvalidRequest_Returns400()
    {
        var request = new CreateStudentRequest("", "Lovelace", new DateOnly(2005, 1, 1), "not-an-email", null);

        var response = await _client.PostAsJsonAsync("/api/students", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        await _repository.DidNotReceive().AddAsync(Arg.Any<Student>(), Arg.Any<CancellationToken>());
    }
}
