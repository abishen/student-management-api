using StudentManagement.Api.Common;
using StudentManagement.Api.Common.Cqrs;
using StudentManagement.Api.Features.Students;
using StudentManagement.Api.Features.Students.CreateStudent;
using StudentManagement.Api.Features.Students.GetStudentById;
using StudentManagement.Api.Features.Students.GetStudents;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IStudentRepository, InMemoryStudentRepository>();
builder.Services.AddScoped<ICommandHandler<CreateStudentCommand, Result<StudentResponse>>, CreateStudentHandler>();
builder.Services.AddScoped<IQueryHandler<GetStudentByIdQuery, StudentResponse?>, GetStudentByIdHandler>();
builder.Services.AddScoped<IQueryHandler<GetStudentsQuery, IReadOnlyList<StudentResponse>>, GetStudentsHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "StudentManagement.Api v1"));
}

app.UseHttpsRedirection();

app.MapCreateStudentEndpoint();
app.MapGetStudentByIdEndpoint();
app.MapGetStudentsEndpoint();

app.Run();

// Exposed so WebApplicationFactory<Program> can bootstrap the app in integration tests.
public partial class Program;
