using System.Windows;
using System;

public class ChatItem
{
    public object Content { get; set; }
    public HorizontalAlignment HorizontalAlignment { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ImagePath { get; set; }
    public DateTime CreationDate { get; set; }
    public bool IsFile { get; set; }
}