using System.ComponentModel.DataAnnotations;

namespace auth_user_service.DTOs
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = null!;

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = null!;
    }
}
