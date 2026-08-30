using StudentManagement.Api.Common.Cqrs;

namespace StudentManagement.Api.Features.Students.GetStudentById;

public sealed class GetStudentByIdHandler(IStudentRepository repository)
    : IQueryHandler<GetStudentByIdQuery, StudentResponse?>
{
    public async Task<StudentResponse?> HandleAsync(GetStudentByIdQuery query, CancellationToken cancellationToken)
    {
        var student = await repository.GetByIdAsync(query.Id, cancellationToken);
        return student is null ? null : StudentResponse.FromStudent(student);
    }
}
