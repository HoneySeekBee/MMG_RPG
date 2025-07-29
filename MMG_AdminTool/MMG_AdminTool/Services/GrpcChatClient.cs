using Grpc.Net.Client;
using System;
using GrpcChat;

namespace MMG_AdminTool.Services
{
    public class GrpcChatClient
    {
        private readonly string _grpcUrl;
        private GrpcChannel? _channel;
        public ChatSync.ChatSyncClient? Client { get; private set; }

        public bool IsConnected => _channel != null;

        public GrpcChatClient(string grpcUrl)
        {
            _grpcUrl = grpcUrl;
        }
        public void EnsureConnected()
        {
            if (_channel != null)
                return;

            Console.WriteLine($"[gRPC] {_grpcUrl} 서버에 연결 시도...");
            _channel = GrpcChannel.ForAddress(_grpcUrl);

            // gRPC 프로토에서 생성된 ChatClient 사용 (예: Chat.proto에서 생성)
            Client = new ChatSync.ChatSyncClient(_channel);

            Console.WriteLine("[gRPC] 연결 성공");
        }
        public async Task<string> AdminChat(string type, string message)
        {
            if (!IsConnected)
                throw new Exception("gRPC 연결이 안 되어있음");

            // AdminChat 호출
            var request = new ChatRequest
            {
                Type = type,
                Message = message
            };

            var reply = await Client.AdminChatAsync(request);

            // 응답 메시지를 반환 (서버에서 처리 후 확인 메시지)
            return reply.Message;
        }
        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}
