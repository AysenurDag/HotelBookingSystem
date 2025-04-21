namespace auth_user_service.Messaging
{
    public interface IMessagePublisher
    {
        void Publish(string message);
    }
}
