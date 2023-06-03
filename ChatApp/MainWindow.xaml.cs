using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace ChatApp
{
    public partial class MainWindow : Window
    {
        public MainWindow() { }

        private void CreateChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_username.Text))
            {
                MessageBox.Show("Вы не указали ваше имя!", "Мессенджер", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ChatWindow chatWindow = new ChatWindow(txt_username.Text.Trim(), true, "127.0.0.1");
            this.Hide();
            chatWindow.Show();
            txt_username.Text = "";
            txt_ip.Text = "";
        }

        private void JoinChatButton_Click(object sender, RoutedEventArgs e)
        {
            IPAddress ipAddress;

            if (string.IsNullOrWhiteSpace(txt_username.Text))
            {
                MessageBox.Show("Вы не указали ваше имя!", "Мессенджер", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txt_ip.Text))
            {
                MessageBox.Show("Вы не указали IP адрес!", "Мессенджер", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (IPAddress.TryParse(txt_ip.Text, out ipAddress))
            {
                try
                {
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(new IPEndPoint(ipAddress, 8888));
                    if (socket.Connected)
                    {
                        ChatWindow chatWindow = new ChatWindow(txt_username.Text.Trim(), false, txt_ip.Text);
                        this.Hide();
                        chatWindow.Show();
                        txt_username.Text = "";
                        txt_ip.Text = "";
                    }
                    else
                    {
                        MessageBox.Show("Сервер недоступен!", "Мессенджер", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (SocketException)
                {
                    MessageBox.Show("Не удалось подключиться к серверу!", "Мессенджер", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
                MessageBox.Show("Некорректный IP адрес!", "Мессенджер", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Window_Closed(object sender, System.EventArgs e) => Application.Current.Shutdown();
    }
}
