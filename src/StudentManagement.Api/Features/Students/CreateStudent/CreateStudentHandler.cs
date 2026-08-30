using StudentManagement.Api.Common;
using StudentManagement.Api.Common.Cqrs;

namespace StudentManagement.Api.Features.Students.CreateStudent;

public sealed class CreateStudentHandler(IStudentRepository repository)
    : ICommandHandler<CreateStudentCommand, Result<StudentResponse>>
{
    public async Task<Result<StudentResponse>> HandleAsync(CreateStudentCommand command, CancellationToken cancellationToken)
    {
        var errors = Validate(command);
        if (errors.Count > 0)
        {
            return Result<StudentResponse>.Failure(errors.ToArray());
        }

        var student = new Student
        {
            Id = Guid.NewGuid(),
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            DateOfBirth = command.DateOfBirth,
            Email = command.Email.Trim(),
            Grade = command.Grade?.Trim()
        };

        var created = await repository.AddAsync(student, cancellationToken);
        return Result<StudentResponse>.Success(StudentResponse.FromStudent(created));
    }

    private static List<string> Validate(CreateStudentCommand command)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(command.FirstName))
        {
            errors.Add("FirstName is required.");
        }

        if (string.IsNullOrWhiteSpace(command.LastName))
        {
            errors.Add("LastName is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Email) || !command.Email.Contains('@'))
        {
            errors.Add("A valid Email is required.");
        }

        if (command.DateOfBirth == default || command.DateOfBirth >= DateOnly.FromDateTime(DateTime.UtcNow))
        {
            errors.Add("DateOfBirth must be a valid date in the past.");
        }

        return errors;
    }
}
