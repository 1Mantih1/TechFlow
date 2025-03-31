using System;

namespace TechFlow.Classes
{
    public class Timesheet
    {
        public int TimesheetId { get; set; }
        public DateTime WorkDate { get; set; }
        public int EmployeeId { get; set; }
        public WorkStatus Status { get; set; }

        public Timesheet(int timesheetId, DateTime workDate, int employeeId, WorkStatus status)
        {
            TimesheetId = timesheetId;
            WorkDate = workDate;
            EmployeeId = employeeId;
            Status = status;
        }
    }
}
