namespace APBDTest1;

public record TeamMemberInfo(string FirstName, string LastName, string Email, List<TaskInfo> AssignedTasks, List<TaskInfo> CreatedTasks);