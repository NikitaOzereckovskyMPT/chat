using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp
{
    public class TcpServer
    {
        private Socket socket;
        private List<Socket> clients = new List<Socket>();
        private List<User> users = new List<User>();
        private readonly CancellationTokenSource _cancellationTokenSource;

        public delegate void MessageReceivedEventHandler(string message);
        public event MessageReceivedEventHandler MessageReceived;

        public TcpServer() => _cancellationTokenSource = new CancellationTokenSource();

        public async Task StartListening()
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ipPoint);
            socket.Listen(1000);
           
            ListenToClients();
        }

        private async Task ListenToClients()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var client = await socket.AcceptAsync();
                clients.Add(client);

                User user = new User()
                {
                    Socket = client,
                    Username = ""
                };

                users.Add(user);

                ReceiveMessage(client, _cancellationTokenSource);
            }

            socket.Close();
        }

        private async Task ReceiveMessage(Socket client, CancellationTokenSource cancellationTokenSource)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                byte[] bytes = new byte[1024];
                await client.ReceiveAsync(bytes, SocketFlags.None);
                var user = users.FirstOrDefault(user => user.Socket == client);

                string message = Encoding.UTF8.GetString(bytes, 0, Array.IndexOf<byte>(bytes, 0));

                if (user.Username == string.Empty)
                {
                    user.Username = message;
                    MessageReceived?.Invoke($"[{DateTime.Now}]\nПользователь [{user.Username}] присоеденился к чату!");

                    string usersname = "<AllUsers>" + string.Join(";", users.Select(u => u.Username));
                    foreach (var item in clients)
                    {
                        SendMessage(item, usersname);
                    }
                    continue;
                }

                if (message.Contains("/disconnect"))
                {
                    client.Close();
                    MessageReceived?.Invoke($"[{DateTime.Now}]\nПользователь [{user?.Username}] покинул чат!");
                    users.Remove(user);
                    clients.Remove(client);

                    string usersname = "<AllUsers>" + string.Join(";", users.Select(u => u.Username));
                    foreach (var item in clients)
                    {
                        SendMessage(item, usersname);
                    }
                    continue;
                }

                string fullmessage = $"[{DateTime.Now}][{user?.Username}]: {Encoding.UTF8.GetString(bytes, 0, Array.IndexOf<byte>(bytes, 0))}";

                foreach (var item in clients)
                {
                    SendMessage(item, fullmessage);
                }
            }

            client.Close();
        }

        private async Task SendMessage(Socket client, string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(bytes, SocketFlags.None);
        }

        public void Stop()
        {
            foreach (var item in clients)
            {
                SendMessage(item, "Сервер больше недоступен!");
            }

            _cancellationTokenSource.Cancel();
            socket.Close();
        }
    }
}