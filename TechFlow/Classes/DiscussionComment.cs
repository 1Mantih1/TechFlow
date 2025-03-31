using System;
using System.Windows;

public class DiscussionComment
{
    public int CommentId { get; set; }
    public string CommentText { get; set; }
    public DateTime CreationDate { get; set; }
    public int DiscussionId { get; set; }
    public int EmployeeId { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ImagePath { get; set; }
    public HorizontalAlignment HorizontalAlignment { get; set; }

    public DiscussionComment(int commentId, string commentText, DateTime creationDate, int discussionId, int employeeId, string firstName, string lastName, string imagePath)
    {
        CommentId = commentId;
        CommentText = commentText;
        CreationDate = creationDate;
        DiscussionId = discussionId;
        EmployeeId = employeeId;

        FirstName = firstName;
        LastName = lastName;
        ImagePath = imagePath;
    }
}
