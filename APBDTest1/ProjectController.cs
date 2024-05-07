using System.Data.SqlClient;

namespace APBDTest1;

public class ProjectController
{
    private SqlConnection _connection;
    
    private ProjectController(string connectionString)
    {
        this._connection = new SqlConnection(connectionString);
    }
    
    public static async Task<ProjectController> Create(string connectionString)
    {
        var controller = new ProjectController(connectionString);
        await controller._connection.OpenAsync();
        return controller;
    }

    public async Task<TaskInfo?> GetTask(int idTask)
    {
        await using SqlCommand command = new SqlCommand("SELECT IdTask, Task.Name, Description, Deadline, IdProject, TaskType.Name, IdAssignedTo, IdCreator FROM Task JOIN TaskType ON Task.IdTaskType = TaskType.IdTaskType WHERE IdTask = @IdTask", this._connection);
        command.Parameters.AddWithValue("@IdTask", idTask);
        await using SqlDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new TaskInfo((int)reader[0], (string)reader[1], (string)reader[2], (DateTime)reader[3], (int)reader[4],
            (string)reader[5], (int)reader[6],
            (int)reader[7]);
    }

    public async Task<TeamMemberInfo?> GetMemberInfo(int idTeamMember)
    {
        List<TaskInfo> tasks = new List<TaskInfo>();
        await using (SqlCommand command = new SqlCommand("SELECT IdTask, Task.Name, Description, Deadline, IdProject, TaskType.Name, IdAssignedTo, IdCreator FROM Task JOIN TaskType ON Task.IdTaskType = TaskType.IdTaskType WHERE IdAssignedTo = @IdAssignedTo ORDER BY DeadLine DESC", this._connection))
        {
            command.Parameters.AddWithValue("@IdAssignedTo", idTeamMember);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tasks.Add(new TaskInfo((int)reader[0], (string)reader[1], (string)reader[2], (DateTime)reader[3],
                    (int)reader[4],
                    (string)reader[5], (int)reader[6],
                    (int)reader[7]));
            }
        }

        await using (SqlCommand command = new SqlCommand("SELECT FirstName, LastName, Email FROM TeamMember WHERE IdTeamMember = @IdTeamMember", this._connection))
        {
            command.Parameters.AddWithValue("@IdTeamMember", idTeamMember);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new TeamMemberInfo((string)reader[0], (string)reader[1], (string)reader[2], tasks);
        }
    }
}