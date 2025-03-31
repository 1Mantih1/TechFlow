using System;

namespace TechFlow.Classes
{
    public class ProjectStage
    {
        public int StageId { get; set; }
        public string StageName { get; set; }
        public string ProjectStageDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }

        public ProjectStage(int stageId, string stageName, string projectStageDescription, DateTime startDate, DateTime? endDate, string status, int projectId, string projectName)
        {
            StageId = stageId;
            StageName = stageName;
            ProjectStageDescription = projectStageDescription;
            StartDate = startDate;
            EndDate = endDate;
            Status = status;
            ProjectId = projectId;
            ProjectName = projectName;
        }
    }

}
