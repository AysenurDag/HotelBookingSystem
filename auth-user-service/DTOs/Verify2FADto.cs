namespace auth_user_service.DTOs
{
    public class Verify2FADto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
