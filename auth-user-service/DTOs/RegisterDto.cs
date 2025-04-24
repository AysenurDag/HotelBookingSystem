using System.ComponentModel.DataAnnotations;

namespace auth_user_service.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Surname { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, Phone]
        public string PhoneNumber { get; set; } = null!;

        [Required, MinLength(6)]
        public string Password { get; set; } = null!;

        public List<string> Roles { get; set; } = new();
    }
}
