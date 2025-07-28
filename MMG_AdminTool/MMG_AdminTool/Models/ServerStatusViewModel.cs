namespace MMG_AdminTool.Models
{
    public class ServerStatusViewModel
    {
        public string Name { get; set; } = "";
        public int Port { get; set; }
        public string Status { get; set; } = ""; // Alive/Dead
        public int ConnectedClients { get; set; }
    }
}
