using System.ComponentModel.DataAnnotations;

namespace LicencjatUG.Server.Models
{
    public class UserLoginDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
