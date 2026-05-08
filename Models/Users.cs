using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;


namespace BIsm2.Models
{
    public class Users : IdentityUser
    {
        public string? FulllName { get; set; }
    }
}
