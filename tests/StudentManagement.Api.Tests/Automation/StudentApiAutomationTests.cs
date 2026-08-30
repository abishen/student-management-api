using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using StudentManagement.Api.Features.Students;
using StudentManagement.Api.Features.Students.CreateStudent;

namespace StudentManagement.Api.Tests.Automation;

// Full end-to-end tests against the real in-memory repository - no mocks/substitutes.
[TestFixture]
public class StudentApiAutomationTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task CreateStudent_ThenGetById_ReturnsTheSameStudent()
    {
        var request = new CreateStudentRequest("Ada", "Lovelace", new DateOnly(2005, 1, 1), "ada@example.com", "Grade 10");

        var createResponse = await _client.PostAsJsonAsync("/api/students", request);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var created = await createResponse.Content.ReadFromJsonAsync<StudentResponse>();
        Assert.That(created, Is.Not.Null);

        var getResponse = await _client.GetAsync(createResponse.Headers.Location);
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var fetched = await getResponse.Content.ReadFromJsonAsync<StudentResponse>();
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Id, Is.EqualTo(created!.Id));
        Assert.That(fetched.FirstName, Is.EqualTo("Ada"));
        Assert.That(fetched.LastName, Is.EqualTo("Lovelace"));
        Assert.That(fetched.Email, Is.EqualTo("ada@example.com"));
        Assert.That(fetched.Grade, Is.EqualTo("Grade 10"));
    }

    [Test]
    public async Task CreateMultipleStudents_ThenGetAll_ReturnsEveryCreatedStudent()
    {
        var first = new CreateStudentRequest("Ada", "Lovelace", new DateOnly(2005, 1, 1), "ada@example.com", "Grade 10");
        var second = new CreateStudentRequest("Alan", "Turing", new DateOnly(1999, 6, 23), "alan@example.com", "Grade 11");

        await _client.PostAsJsonAsync("/api/students", first);
        await _client.PostAsJsonAsync("/api/students", second);

        var getAllResponse = await _client.GetAsync("/api/students");
        Assert.That(getAllResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var students = await getAllResponse.Content.ReadFromJsonAsync<List<StudentResponse>>();
        Assert.That(students, Is.Not.Null);
        Assert.That(students!.Select(s => s.FirstName), Is.EquivalentTo(new[] { "Ada", "Alan" }));
    }

    [Test]
    public async Task GetById_WhenStudentWasNeverCreated_Returns404()
    {
        var response = await _client.GetAsync($"/api/students/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CreateStudent_WithInvalidPayload_Returns400AndIsNotPersisted()
    {
        var invalidRequest = new CreateStudentRequest("", "Lovelace", new DateOnly(2005, 1, 1), "not-an-email", null);

        var createResponse = await _client.PostAsJsonAsync("/api/students", invalidRequest);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var getAllResponse = await _client.GetAsync("/api/students");
        var students = await getAllResponse.Content.ReadFromJsonAsync<List<StudentResponse>>();
        Assert.That(students, Is.Empty);
    }

    [Test]
    public async Task GetAll_WhenNoStudentsCreated_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/students");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var students = await response.Content.ReadFromJsonAsync<List<StudentResponse>>();
        Assert.That(students, Is.Empty);
    }
}
