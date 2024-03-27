using Microsoft.AspNetCore.Mvc;

namespace broodjeszaak.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsApproved { get; set; }
    }
}
