namespace StudentManagement.Api.Features.Students;

public interface IStudentRepository
{
    Task<Student> AddAsync(Student student, CancellationToken cancellationToken);

    Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken);
}
