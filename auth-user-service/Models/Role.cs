namespace auth_user_service.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;   // "ROLE_USER", "ROLE_HOTEL_OWNER", "ROLE_ADMIN"
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
