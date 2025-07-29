using Microsoft.AspNetCore.Mvc;
using MMG_AdminTool.Services;

namespace MMG_AdminTool.Controllers
{
    public class ChatController : Controller
    {
        private readonly GrpcChatClient _grpcClient;
        public ChatController(GrpcChatClient grpcClient)
        {
            _grpcClient = grpcClient;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send(string type, string message)
        {
            try
            {
                // gRPC 서버 연결 (처음 호출 시에만)
                _grpcClient.EnsureConnected();

                // gRPC 호출
                var result = await _grpcClient.AdminChat(type, message);

                TempData["Result"] = $"전송 성공: {result}";
            }
            catch (Exception ex)
            {
                TempData["Result"] = $"전송 실패: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
