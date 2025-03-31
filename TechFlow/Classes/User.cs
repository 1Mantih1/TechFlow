using System;

public class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    public string Email { get; set; }
    public string ImagePath { get; set; }

    public User() { }

    public User(int userId, string firstName, string lastName, DateTime dateOfBirth,
                string login, string password, string address, string phone, int roleId, string email, string imagePath)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Login = login;
        Password = password;
        Phone = phone;
        Address = address;
        RoleId = roleId;
        Email = email;
        ImagePath = imagePath;
    }
}
