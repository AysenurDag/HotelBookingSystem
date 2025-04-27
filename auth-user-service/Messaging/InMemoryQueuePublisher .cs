using System.Collections.Concurrent;

namespace auth_user_service.Messaging
{
    public class InMemoryQueuePublisher : IMessagePublisher
    {
        public ConcurrentQueue<string> Messages { get; } = new();

        public void Publish(string message)
        {
            Messages.Enqueue(message);
        }
    }
}
