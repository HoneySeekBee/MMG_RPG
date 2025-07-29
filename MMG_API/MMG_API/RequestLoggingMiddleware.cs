using StackExchange.Redis;
using System.Text.Json;

namespace MMG_API
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDatabase _redis;

        public RequestLoggingMiddleware(RequestDelegate next, IDatabase redis)
        {
            _next = next;
            _redis = redis;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 요청 정보
            var log = new
            {
                Path = context.Request.Path.ToString(),
                Method = context.Request.Method,
                Ip = context.Connection.RemoteIpAddress?.ToString(),
                Time = DateTime.UtcNow
            };

            // Redis 리스트에 푸시
            Console.WriteLine($"[Invoe] : {log.Path}, {log.Ip}, {log.Time} ");
            await _redis.ListRightPushAsync("API:RequestLogs", JsonSerializer.Serialize(log));

            await _next(context); // 다음 미들웨어로 넘기기
        }
    }
}
