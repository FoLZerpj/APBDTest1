using APBDTest1;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

const string connectionString = "Server=(localdb)\\MSSQLLocalDB";
IProjectController controller = await ProjectController.Create(connectionString);

app.MapGet("/api/member/{memberId}", async (int memberId) =>
    {
        var member = await controller.GetMemberInfoAsync(memberId);
        if (member == null)
        {
            return TypedResults.NotFound("Member with specified memberId wasn't found");
        }

        return (IResult)TypedResults.Ok(member);
    })
    .WithName("GetTeamMemberInformation")
    .WithOpenApi();

app.MapGet("/api/tasks/{taskId}", async (int taskId) =>
    {
        var task = await controller.GetTaskAsync(taskId);
        if (task == null)
        {
            return TypedResults.NotFound("Task with specified taskId wasn't found");
        }

        return (IResult)TypedResults.Ok(task);
    })
    .WithName("GetTaskInformation")
    .WithOpenApi();

app.MapDelete("/api/tasks/{taskId}", async (int taskId) =>
    {
        await controller.DeleteTaskAsync(taskId);
    })
    .WithName("GetTaskInformation")
    .WithOpenApi();

app.MapDelete("/api/project/{projectId}", async (int projectId) =>
    {
        await controller.DeleteProjectAsync(projectId);
    })
    .WithName("DeleteProject")
    .WithOpenApi();

app.Run();