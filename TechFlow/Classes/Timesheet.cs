using System;

namespace TechFlow.Classes
{
    public class Timesheet
    {
        public int TimesheetId { get; }
        public DateTime WorkDate { get; }
        public int EmployeeId { get; }
        public WorkStatus Status { get; }
        public DateTime? CheckInTime { get; }

        public Timesheet(int timesheetId, DateTime workDate, int employeeId, WorkStatus status, DateTime? checkInTime = null)
        {
            TimesheetId = timesheetId;
            WorkDate = workDate;
            EmployeeId = employeeId;
            Status = status;
            CheckInTime = checkInTime;
        }
    }
}
