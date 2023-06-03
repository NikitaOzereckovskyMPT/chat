using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp
{
    public class TcpClient
    {
        private Socket client;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public delegate void MessageReceivedHandler(string message);
        public event MessageReceivedHandler MessageReceived;

        public TcpClient()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource = cancellationTokenSource;
        }

        public async Task ConnectAsync(string ip, int port)
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(ip, port);

            await Task.Run(() => ReceiveMessage());
        }

        private async Task ReceiveMessage()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                byte[] bytes = new byte[1024];
                await client.ReceiveAsync(bytes, SocketFlags.None);
                string message = Encoding.UTF8.GetString(bytes, 0, Array.IndexOf<byte>(bytes, 0));

                MessageReceived?.Invoke(message);
            }

            client.Close();
        }

        public async Task SendMessage(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(bytes, SocketFlags.None);
        }

        public void Stop() => _cancellationTokenSource.Cancel();
    }
}
