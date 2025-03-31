using System;

namespace TechFlow.Classes
{
    public class Discussion
    {
        public int DiscussionId { get; set; }
        public string DiscussionName { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public int TaskId { get; set; }

        public Discussion(int discussionId, string discussionName, DateTime creationDate, DateTime? completionDate, int taskId)
        {
            DiscussionId = discussionId;
            DiscussionName = discussionName;
            CreationDate = creationDate;
            CompletionDate = completionDate;
            TaskId = taskId;
        }
    }
}
