using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechFlow.Classes
{
    public class Client
    {
        public int ClientId { get; set; }
        public string OrganizationName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Login { get; set; }

        public Client(int clientId, string organizationName, string firstName,
                     string lastName, string email, string phone, string login)
        {
            ClientId = clientId;
            OrganizationName = organizationName;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
            Login = login;
        }
    }
}
