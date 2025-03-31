namespace TechFlow.Classes
{
    public class WorkStatus
    {
        public string StatusCode { get; set; }
        public string Description { get; set; }

        public WorkStatus(string statusCode, string description)
        {
            StatusCode = statusCode;
            Description = description;
        }
    }
}
