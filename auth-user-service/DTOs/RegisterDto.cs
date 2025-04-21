using System.ComponentModel.DataAnnotations;

namespace auth_user_service.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        public List<string> Roles { get; set; }
    }
}
