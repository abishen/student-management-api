using StudentManagement.Api.Common;
using StudentManagement.Api.Common.Cqrs;

namespace StudentManagement.Api.Features.Students.CreateStudent;

public static class CreateStudentEndpoint
{
    public static IEndpointRouteBuilder MapCreateStudentEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/students", async (
                CreateStudentRequest request,
                ICommandHandler<CreateStudentCommand, Result<StudentResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateStudentCommand(request.FirstName, request.LastName, request.DateOfBirth, request.Email, request.Grade);
                var result = await handler.HandleAsync(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Created($"/api/students/{result.Value!.Id}", result.Value)
                    : Results.BadRequest(new { errors = result.Errors });
            })
            .WithName("CreateStudent")
            .WithTags("Students")
            .Produces<StudentResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        return app;
    }
}
