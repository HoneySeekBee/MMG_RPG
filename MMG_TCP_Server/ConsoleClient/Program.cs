using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConsoleClient
{
    internal class Program
    {
        static void Main()
        {
            ClientPacketManager.Register(); // 반드시 등록

            Connector connector = new Connector();
            connector.Connect(GetEndPoint(), () => new ClientSession());

            while (true)
            {
                Thread.Sleep(100);
            }
        }
        static IPEndPoint GetEndPoint()
        {
            string host = Dns.GetHostName();
            IPAddress ipAddr = Dns.GetHostEntry(host)
    .AddressList
    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
            return endPoint;
        }
        static void ConnectChatServer()
        {

        }
    }
}
