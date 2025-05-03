using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechFlow.Classes
{
    public class TimesheetRecord
    {
        public int EmployeeId { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public string StatusCode { get; set; }
    }
}
