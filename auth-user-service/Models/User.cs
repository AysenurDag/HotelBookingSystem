namespace auth_user_service.Models
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }         
        public string Surname { get; private set; }      
        public string Email { get; private set; }
        public string PhoneNumber { get; private set; }  
        public string PasswordHash { get; private set; }
        public DateTime CreatedDate { get; private set; }

        private List<object> _domainEvents = new List<object>();
        public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public User(string name, string surname, string email, string phoneNumber, string passwordHash)
        {
            Id = Guid.NewGuid();
            Name = name;
            Surname = surname;
            Email = email;
            PhoneNumber = phoneNumber;
            PasswordHash = passwordHash;
            CreatedDate = DateTime.UtcNow;

            _domainEvents.Add(new UserRegisteredEvent(Id, email));
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }

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
