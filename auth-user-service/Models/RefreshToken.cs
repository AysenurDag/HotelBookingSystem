using System.ComponentModel.DataAnnotations;

namespace auth_user_service.Models
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Token { get; set; } = null!;

        [Required]
        public string JwtId { get; set; } = null!;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsUsed { get; set; } = false;

        public bool IsRevoked { get; set; } = false;

        [Required]
        public string UserId { get; set; } = null!;
    }
}
