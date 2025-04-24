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
        public string ProjectType { get; set; }
        public decimal? Budget { get; set; }
        public string Requirements { get; set; }
        public bool IsUrgent { get; set; }
        public bool IsConfidential { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ClientName { get; set; }
        public string Status { get; set; }

        public Project(int projectId, string projectName, string projectDescription, DateTime startDate,
                       DateTime? endDate, int clientId, string projectType, decimal? budget, string requirements,
                       bool isUrgent, bool isConfidential, DateTime createdAt, string clientName, string status)
        {
            ProjectId = projectId;
            ProjectName = projectName;
            ProjectDescription = projectDescription;
            StartDate = startDate;
            EndDate = endDate;
            ClientId = clientId;
            ProjectType = projectType;
            Budget = budget;
            Requirements = requirements;
            IsUrgent = isUrgent;
            IsConfidential = isConfidential;
            CreatedAt = createdAt;
            ClientName = clientName;
            Status = status;
        }
    }
}
