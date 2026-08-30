using StudentManagement.Api.Common.Cqrs;

namespace StudentManagement.Api.Features.Students.GetStudents;

public sealed class GetStudentsHandler(IStudentRepository repository)
    : IQueryHandler<GetStudentsQuery, IReadOnlyList<StudentResponse>>
{
    public async Task<IReadOnlyList<StudentResponse>> HandleAsync(GetStudentsQuery query, CancellationToken cancellationToken)
    {
        var students = await repository.GetAllAsync(cancellationToken);
        return students.Select(StudentResponse.FromStudent).ToList();
    }
}
