using NSubstitute;
using StudentManagement.Api.Features.Students;
using StudentManagement.Api.Features.Students.CreateStudent;

namespace StudentManagement.Api.Tests.Features.Students.CreateStudent;

[TestFixture]
public class CreateStudentHandlerTests
{
    private IStudentRepository _repository = null!;
    private CreateStudentHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IStudentRepository>();
        _handler = new CreateStudentHandler(_repository);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ReturnsSuccessAndPersistsStudent()
    {
        var command = new CreateStudentCommand("Ada", "Lovelace", new DateOnly(2005, 1, 1), "ada@example.com", "Grade 10");
        _repository.AddAsync(Arg.Any<Student>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Student>()));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value!.FirstName, Is.EqualTo("Ada"));
        Assert.That(result.Value!.Email, Is.EqualTo("ada@example.com"));
        await _repository.Received(1).AddAsync(Arg.Is<Student>(s => s.FirstName == "Ada"), Arg.Any<CancellationToken>());
    }

    [TestCase("", "Lovelace", "ada@example.com")]
    [TestCase("Ada", "", "ada@example.com")]
    [TestCase("Ada", "Lovelace", "not-an-email")]
    public async Task HandleAsync_WithInvalidCommand_ReturnsFailureAndDoesNotPersist(string firstName, string lastName, string email)
    {
        var command = new CreateStudentCommand(firstName, lastName, new DateOnly(2005, 1, 1), email, null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Is.Not.Empty);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Student>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleAsync_WithFutureDateOfBirth_ReturnsFailure()
    {
        var command = new CreateStudentCommand("Ada", "Lovelace", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), "ada@example.com", null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Does.Contain("DateOfBirth must be a valid date in the past."));
    }
}
