namespace StudentManagement.Api.Features.Students.CreateStudent;

public sealed record CreateStudentCommand(string FirstName, string LastName, DateOnly DateOfBirth, string Email, string? Grade);
