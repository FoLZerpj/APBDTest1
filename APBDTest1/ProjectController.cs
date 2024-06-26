﻿using System.Data.SqlClient;

namespace APBDTest1;

public class ProjectController: IProjectController
{
    private readonly SqlConnection _connection;
    
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

    public async Task<TaskInfo?> GetTaskAsync(int idTask)
    {
        await using SqlCommand command = new SqlCommand("SELECT Task.Name, Description, Task.Deadline, Project.Name, TaskType.Name FROM Task JOIN TaskType ON Task.IdTaskType = TaskType.IdTaskType JOIN Project ON Task.IdProject = Project.IdProject WHERE IdTask = @IdTask", this._connection);
        command.Parameters.AddWithValue("@IdTask", idTask);
        await using SqlDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new TaskInfo((string)reader[0], (string)reader[1], (DateTime)reader[2], (string)reader[3],
            (string)reader[4]);
    }
    
    public async Task DeleteTaskAsync(int idTask)
    {
        await using SqlCommand command = new SqlCommand("DELETE FROM Task WHERE IdTask = @IdTask", this._connection);
        command.Parameters.AddWithValue("@IdTask", idTask);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<TeamMemberInfo?> GetMemberInfoAsync(int idTeamMember)
    {
        List<TaskInfo> assignedTasks = new List<TaskInfo>();
        await using (SqlCommand command = new SqlCommand("SELECT Task.Name, Description, Task.Deadline, Project.Name, TaskType.Name FROM Task JOIN TaskType ON Task.IdTaskType = TaskType.IdTaskType JOIN Project ON Task.IdProject = Project.IdProject WHERE IdAssignedTo = @IdAssignedTo ORDER BY DeadLine DESC", this._connection))
        {
            command.Parameters.AddWithValue("@IdAssignedTo", idTeamMember);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                assignedTasks.Add(new TaskInfo((string)reader[0], (string)reader[1], (DateTime)reader[2],
                    (string)reader[3],
                    (string)reader[4]));
            }
        }
        
        List<TaskInfo> createdTasks = new List<TaskInfo>();
        await using (SqlCommand command = new SqlCommand("SELECT Task.Name, Description, Task.Deadline, Project.Name, TaskType.Name FROM Task JOIN TaskType ON Task.IdTaskType = TaskType.IdTaskType JOIN Project ON Task.IdProject = Project.IdProject WHERE IdCreator = @IdCreator ORDER BY DeadLine DESC", this._connection))
        {
            command.Parameters.AddWithValue("@IdCreator", idTeamMember);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                createdTasks.Add(new TaskInfo((string)reader[0], (string)reader[1], (DateTime)reader[2],
                    (string)reader[3],
                    (string)reader[4]));
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

            return new TeamMemberInfo((string)reader[0], (string)reader[1], (string)reader[2], assignedTasks, createdTasks);
        }
    }

    public async Task DeleteProjectAsync(int idProject)
    {
        await using SqlCommand command = new SqlCommand("BEGIN TRANSACTION;" +
                                                        "DELETE FROM Task WHERE IdProject = @IdProject;" +
                                                        "DELETE FROM Project WHERE IdProject = @IdProject;" +
                                                        "COMMIT;", this._connection);
        command.Parameters.AddWithValue("@IdProject", idProject);
        await command.ExecuteNonQueryAsync();
    }
}