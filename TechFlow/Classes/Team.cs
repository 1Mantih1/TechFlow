using System;

namespace TechFlow.Classes
{
    public class Team
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string TeamDescription { get; set; }
        public DateTime OrganizationDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        public Team(int teamId, string teamName, string teamDescription, DateTime organizationDate, DateTime? completionDate)
        {
            TeamId = teamId;
            TeamName = teamName;
            TeamDescription = teamDescription;
            OrganizationDate = organizationDate;
            CompletionDate = completionDate;
        }
    }
}
