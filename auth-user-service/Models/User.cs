namespace auth_user_service.Models
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public DateTime CreatedDate { get; private set; }

        // Domain event listesini saklayacağınız alan
        private List<object> _domainEvents = new List<object>();
        public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

        public User(string email, string passwordHash)
        {
            Id = Guid.NewGuid();
            Email = email;
            PasswordHash = passwordHash;
            CreatedDate = DateTime.UtcNow;

            // Domain event ekleniyor: Kullanıcı kayıt edildi
            _domainEvents.Add(new UserRegisteredEvent(Id, email));
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }

    // Domain/Events/UserRegisteredEvent.cs
    public class UserRegisteredEvent
    {
        public Guid UserId { get; }
        public string Email { get; }

        public UserRegisteredEvent(Guid userId, string email)
        {
            UserId = userId;
            Email = email;
        }
    }
}
