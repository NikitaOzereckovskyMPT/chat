using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace ChatApp
{
    public partial class ChatWindow : Window
    {
        private readonly bool isServer;
        private TcpServer tcpServer;
        private TcpClient tcpClient;
        private  string _IPAddress;
        private bool IsLogsVisible = false;
        private bool IsMessageEnter = true;

        public ChatWindow(string username, bool IsServer, string IP)
        {
            InitializeComponent();

            if (IsServer)
            {
                tcpServer = new TcpServer();
                tcpServer.StartListening();
                tcpServer.MessageReceived += Server_MessageReceived;
            }
            else
                chd_LogVisible.Visibility = Visibility.Collapsed;

            tcpClient = new TcpClient();
            tcpClient.MessageReceived += TcpClient_MessageReceived;
            _IPAddress = IP;
            ConnectToServerAsync();
            tcpClient.SendMessage(username);
            isServer = IsServer;
        }

        private void Server_MessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txt_Logs.Items.Add(message);
            });
        }

        private async Task ConnectToServerAsync() => await tcpClient.ConnectAsync(_IPAddress, 8888);

        private async void SendMessageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!IsMessageEnter) return;

            if (!string.IsNullOrWhiteSpace(txt_message.Text))
            {
                if (txt_message.Text.Contains("/disconnect"))
                {

                    CloseServerOrClient();
                    return;
                }

                await tcpClient.SendMessage(txt_message.Text.Trim());
                txt_message.Text = string.Empty;
            }
            
        }

        private void TcpClient_MessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (message.StartsWith("Сервер больше недоступен!"))
                {
                    txt_messageList.Items.Add(message);
                    IsMessageEnter = false;
                    return;
                }
                if (message.StartsWith("<AllUsers>"))
                {
                    txt_UsernameList.Items.Clear();

                    var listUsers = message.Replace("<AllUsers>", "").Split(";", StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < listUsers.Length; i++)
                    {
                        txt_UsernameList.Items.Add($"[{listUsers[i]}]");
                    }
                }
                else
                {
                    txt_messageList.Items.Add(message);
                }
            });
        }

        private void chd_LogVisible_Checked(object sender, RoutedEventArgs e)
        {
            IsLogsVisible ^= true;
            if (IsLogsVisible)
            {
                txt_Logs.Visibility = Visibility.Visible;
                txt_UsernameList.Visibility = Visibility.Collapsed;
            }
            else
            {
                txt_Logs.Visibility = Visibility.Collapsed;
                txt_UsernameList.Visibility = Visibility.Visible;
            }
        }

        private void Window_Closed(object sender, System.EventArgs e) => CloseServerOrClient();

        private void CloseServerOrClient()
        {
            if (isServer)
            {
                tcpServer.Stop();
            }
            else
            {
                tcpClient.SendMessage("/disconnect");
                tcpClient.Stop();
            }


            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow mainWindow)
                {
                    mainWindow.Show();
                    break;
                }
            }

            this.Close();
        }
    }
}
