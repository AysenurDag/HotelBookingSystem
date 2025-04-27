using System.ComponentModel.DataAnnotations;

namespace auth_user_service.DTOs
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
