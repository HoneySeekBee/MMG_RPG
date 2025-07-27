using Grpc.Core;
using GrpcChat;
using MMG_Chat_Server.Room;

namespace MMG_Chat_Server.Protos
{
    public class ChatSyncService : ChatSync.ChatSyncBase
    {
        public override Task<TestHelloReply> TestHello(TestHelloRequest request, ServerCallContext context)
        {
            Console.WriteLine(request.Message); // gRPC 호출되면 출력
            return Task.FromResult(new TestHelloReply
            {
                Reply = "Hello from ChatServer!"
            });
        }

        // ChatRoom 생성하기 
        // ChatRoom의 방 번호
        public override Task<CreateRoomResponse> CreateRoom(CreateRoomRequest request, ServerCallContext context )
        {
            Console.WriteLine($"[CreateRoom] : {request.RoomId}");
            ChatRoomManager.Instance.GetOrCreateRoom(request.RoomId);
            return Task.FromResult(new CreateRoomResponse
            {
                // 채팅방을 생성한다. 
                RoomId = request.RoomId,
            });
        }

    }
}
