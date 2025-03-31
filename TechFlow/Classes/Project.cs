using System;

namespace TechFlow.Classes
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string Status { get; set; }

        public Project(int projectId, string projectName, string projectDescription, DateTime startDate, DateTime? endDate, int clientId, string clientName, string status)
        {
            ProjectId = projectId;
            ProjectName = projectName;
            ProjectDescription = projectDescription;
            StartDate = startDate;
            EndDate = endDate;
            ClientId = clientId;
            ClientName = clientName;
            Status = status;
        }
    }
}
