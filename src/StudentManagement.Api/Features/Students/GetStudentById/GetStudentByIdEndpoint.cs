using StudentManagement.Api.Common.Cqrs;

namespace StudentManagement.Api.Features.Students.GetStudentById;

public static class GetStudentByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetStudentByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/students/{id:guid}", async (
                Guid id,
                IQueryHandler<GetStudentByIdQuery, StudentResponse?> handler,
                CancellationToken cancellationToken) =>
            {
                var response = await handler.HandleAsync(new GetStudentByIdQuery(id), cancellationToken);
                return response is null ? Results.NotFound() : Results.Ok(response);
            })
            .WithName("GetStudentById")
            .WithTags("Students")
            .Produces<StudentResponse>()
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
