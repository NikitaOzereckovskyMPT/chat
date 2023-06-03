using System.Net.Sockets;

namespace ChatApp
{
    public class User
    {
        public string Username { get; set; }
        public Socket Socket { get; set; }
    }
}
