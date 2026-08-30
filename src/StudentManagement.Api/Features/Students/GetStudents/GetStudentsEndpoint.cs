using StudentManagement.Api.Common.Cqrs;

namespace StudentManagement.Api.Features.Students.GetStudents;

public static class GetStudentsEndpoint
{
    public static IEndpointRouteBuilder MapGetStudentsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/students", async (
                IQueryHandler<GetStudentsQuery, IReadOnlyList<StudentResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var response = await handler.HandleAsync(new GetStudentsQuery(), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("GetStudents")
            .WithTags("Students")
            .Produces<IReadOnlyList<StudentResponse>>();

        return app;
    }
}
