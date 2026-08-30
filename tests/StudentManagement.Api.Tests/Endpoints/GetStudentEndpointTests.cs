using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using StudentManagement.Api.Features.Students;

namespace StudentManagement.Api.Tests.Endpoints;

[TestFixture]
public class GetStudentEndpointTests
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
    public async Task GetById_WhenStudentExists_Returns200WithStudent()
    {
        var student = new Student
        {
            Id = Guid.NewGuid(),
            FirstName = "Grace",
            LastName = "Hopper",
            DateOfBirth = new DateOnly(1990, 5, 20),
            Email = "grace@example.com",
            Grade = "Grade 12"
        };
        _repository.GetByIdAsync(student.Id, Arg.Any<CancellationToken>()).Returns(student);

        var response = await _client.GetAsync($"/api/students/{student.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<StudentResponse>();
        Assert.That(body!.FirstName, Is.EqualTo("Grace"));
    }

    [Test]
    public async Task GetById_WhenStudentDoesNotExist_Returns404()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Student?)null);

        var response = await _client.GetAsync($"/api/students/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetAll_ReturnsListOfStudents()
    {
        var students = new List<Student>
        {
            new()
            {
                Id = Guid.NewGuid(), FirstName = "Ada", LastName = "Lovelace",
                DateOfBirth = new DateOnly(2000, 1, 1), Email = "ada@example.com"
            }
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns((IReadOnlyList<Student>)students);

        var response = await _client.GetAsync("/api/students");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<List<StudentResponse>>();
        Assert.That(body, Has.Count.EqualTo(1));
    }
}
