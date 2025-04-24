using System;

namespace TechFlow.Models
{
    public class ProjectAddStageFromDb
    {
        public int StageId { get; set; }
        public string StageName { get; set; }
        public string ProjectStageDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int StatusId { get; set; }
        public int ProjectId { get; set; }

        public ProjectAddStageFromDb(int stageId, string stageName, string projectStageDescription,
                                   DateTime startDate, DateTime? endDate, int statusId, int projectId)
        {
            StageId = stageId;
            StageName = stageName;
            ProjectStageDescription = projectStageDescription;
            StartDate = startDate;
            EndDate = endDate;
            StatusId = statusId;
            ProjectId = projectId;
        }
    }
}