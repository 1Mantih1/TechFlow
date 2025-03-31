using System;

namespace TechFlow.Classes
{
    public class TeamEmployee
    {
        public int TeamEmployeeId { get; set; }
        public int EmployeeRoleId { get; set; }
        public int TeamId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeRoleName { get; set; }
        public string TeamName { get; set; }
        public string EmployeeName { get; set; }
        public string ImagePath { get; set; }

        public TeamEmployee(int teamEmployeeId, int employeeRoleId, int teamId, int employeeId,
                            string employeeRoleName, string teamName, string employeeName, string imagePath)
        {
            TeamEmployeeId = teamEmployeeId;
            EmployeeRoleId = employeeRoleId;
            TeamId = teamId;
            EmployeeId = employeeId;
            EmployeeRoleName = employeeRoleName;
            TeamName = teamName;
            EmployeeName = employeeName;
            ImagePath = imagePath;
        }
    }
}
