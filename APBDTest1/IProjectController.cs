namespace APBDTest1;

public interface IProjectController
{
    public Task<TaskInfo?> GetTaskAsync(int idTask);
    public Task DeleteTaskAsync(int idTask);
    public Task<TeamMemberInfo?> GetMemberInfoAsync(int idTeamMember);
    public Task DeleteProjectAsync(int idProject);
}