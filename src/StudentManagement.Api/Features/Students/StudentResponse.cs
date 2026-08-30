namespace StudentManagement.Api.Features.Students;

public sealed record StudentResponse(Guid Id, string FirstName, string LastName, DateOnly DateOfBirth, string Email, string? Grade)
{
    public static StudentResponse FromStudent(Student student) =>
        new(student.Id, student.FirstName, student.LastName, student.DateOfBirth, student.Email, student.Grade);
}
