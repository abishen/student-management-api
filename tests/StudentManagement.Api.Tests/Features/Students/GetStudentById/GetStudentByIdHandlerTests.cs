using NSubstitute;
using StudentManagement.Api.Features.Students;
using StudentManagement.Api.Features.Students.GetStudentById;

namespace StudentManagement.Api.Tests.Features.Students.GetStudentById;

[TestFixture]
public class GetStudentByIdHandlerTests
{
    private IStudentRepository _repository = null!;
    private GetStudentByIdHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IStudentRepository>();
        _handler = new GetStudentByIdHandler(_repository);
    }

    [Test]
    public async Task HandleAsync_WhenStudentExists_ReturnsMappedResponse()
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

        var response = await _handler.HandleAsync(new GetStudentByIdQuery(student.Id), CancellationToken.None);

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Id, Is.EqualTo(student.Id));
        Assert.That(response.FirstName, Is.EqualTo("Grace"));
    }

    [Test]
    public async Task HandleAsync_WhenStudentDoesNotExist_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Student?)null);

        var response = await _handler.HandleAsync(new GetStudentByIdQuery(id), CancellationToken.None);

        Assert.That(response, Is.Null);
    }
}
