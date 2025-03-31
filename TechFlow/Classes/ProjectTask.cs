using System;

namespace TechFlow.Classes
{
    public class ProjectTask
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } 
        public int StageId { get; set; }
        public string StageName { get; set; } 
        public int TeamId { get; set; }
        public string TeamName { get; set; }

        public ProjectTask(int taskId, string taskName, string taskDescription, DateTime startDate, DateTime? endDate,
                           int statusId, string statusName, int stageId, string stageName, int teamId, string teamName)
        {
            TaskId = taskId;
            TaskName = taskName;
            TaskDescription = taskDescription;
            StartDate = startDate;
            EndDate = endDate;
            StatusId = statusId;
            StatusName = statusName;
            StageId = stageId;
            StageName = stageName;
            TeamId = teamId;
            TeamName = teamName;
        }
    }
}
