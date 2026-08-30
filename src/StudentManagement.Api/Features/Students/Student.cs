namespace StudentManagement.Api.Features.Students;

public sealed record Student
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string Email { get; init; }
    public string? Grade { get; init; }
}
