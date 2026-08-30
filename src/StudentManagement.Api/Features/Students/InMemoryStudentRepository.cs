using System.Collections.Concurrent;

namespace StudentManagement.Api.Features.Students;

// Simple in-process store; swap for a real persistence implementation when one is introduced.
public sealed class InMemoryStudentRepository : IStudentRepository
{
    private readonly ConcurrentDictionary<Guid, Student> _students = new();

    public Task<Student> AddAsync(Student student, CancellationToken cancellationToken)
    {
        _students[student.Id] = student;
        return Task.FromResult(student);
    }

    public Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _students.TryGetValue(id, out var student);
        return Task.FromResult(student);
    }

    public Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Student> all = _students.Values.ToList();
        return Task.FromResult(all);
    }
}
