using Grpc.Net.Client;
using GrpcChat;
using System.Net.Sockets;
using System.Net;

namespace GameServer.ChatServer
{
    public class ChatServerClient
    {
        private readonly ChatSync.ChatSyncClient _client;
        public ChatServerClient()
        {
            var handler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            };

            var channel = GrpcChannel.ForAddress(
    "http://192.168.219.70:7779",
                new GrpcChannelOptions
                {
                    HttpHandler = handler
                });


            //var channel = GrpcChannel.ForAddress($"http://{ip}:7779");
            _client = new ChatSync.ChatSyncClient(channel);
        }
        public async Task CreateRoomAsync(int roomId)
        {
            Console.WriteLine($"[MainServer] gRPC CreateRoom : {roomId}");
            var reply = await _client.CreateRoomAsync(new CreateRoomRequest { RoomId = roomId });
        }

        public async Task TestHelloAsync(string message)
        {
            var reply = await _client.TestHelloAsync(new TestHelloRequest { Message = message });
            Console.WriteLine($"[MainServer] gRPC reply: {reply.Reply}");
        }
        static IPAddress GetLocalIp()
        {
            string host = Dns.GetHostName();
            var ipAddr = Dns.GetHostEntry(host)
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            return ipAddr;
        }

    }
}
