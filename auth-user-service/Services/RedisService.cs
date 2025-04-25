using StackExchange.Redis;

namespace auth_user_service.Services
{
    public class RedisService
    {
        private readonly IDatabase _db;

        public RedisService(IConfiguration config)
        {
            var redis = ConnectionMultiplexer.Connect(config.GetConnectionString("RedisConnection"));
            _db = redis.GetDatabase();
        }

        public void AddTokenToBlacklist(string token, TimeSpan expiration)
        {
            _db.StringSet(token, "blacklisted", expiration);
        }

        public bool IsTokenBlacklisted(string token)
        {
            return _db.KeyExists(token);
        }
    }
}
