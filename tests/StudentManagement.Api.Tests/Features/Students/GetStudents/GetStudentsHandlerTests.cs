using NSubstitute;
using StudentManagement.Api.Features.Students;
using StudentManagement.Api.Features.Students.GetStudents;

namespace StudentManagement.Api.Tests.Features.Students.GetStudents;

[TestFixture]
public class GetStudentsHandlerTests
{
    private IStudentRepository _repository = null!;
    private GetStudentsHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IStudentRepository>();
        _handler = new GetStudentsHandler(_repository);
    }

    [Test]
    public async Task HandleAsync_ReturnsAllStudentsMappedToResponses()
    {
        var students = new List<Student>
        {
            new()
            {
                Id = Guid.NewGuid(), FirstName = "Ada", LastName = "Lovelace",
                DateOfBirth = new DateOnly(2000, 1, 1), Email = "ada@example.com", Grade = "Grade 10"
            },
            new()
            {
                Id = Guid.NewGuid(), FirstName = "Alan", LastName = "Turing",
                DateOfBirth = new DateOnly(1999, 6, 23), Email = "alan@example.com", Grade = "Grade 11"
            }
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns((IReadOnlyList<Student>)students);

        var responses = await _handler.HandleAsync(new GetStudentsQuery(), CancellationToken.None);

        Assert.That(responses, Has.Count.EqualTo(2));
        Assert.That(responses.Select(r => r.FirstName), Is.EquivalentTo(new[] { "Ada", "Alan" }));
    }

    [Test]
    public async Task HandleAsync_WhenNoStudents_ReturnsEmptyList()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns((IReadOnlyList<Student>)new List<Student>());

        var responses = await _handler.HandleAsync(new GetStudentsQuery(), CancellationToken.None);

        Assert.That(responses, Is.Empty);
    }
}
