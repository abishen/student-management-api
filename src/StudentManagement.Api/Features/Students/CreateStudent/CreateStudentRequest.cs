namespace StudentManagement.Api.Features.Students.CreateStudent;

public sealed record CreateStudentRequest(string FirstName, string LastName, DateOnly DateOfBirth, string Email, string? Grade);
