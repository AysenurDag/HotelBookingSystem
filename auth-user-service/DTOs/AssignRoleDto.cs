using System.ComponentModel.DataAnnotations;

namespace auth_user_service.DTOs
{
    public class AssignRoleDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Role { get; set; } = null!;
    }
}
