using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TechFlow.Windows
{
    public partial class CustomMessageBox : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.No;

        public CustomMessageBox()
        {
            InitializeComponent();
        }

        public static MessageBoxResult Show(string message, string title = "Сообщение")
        {
            var dialog = new CustomMessageBox() { Title = title };
            dialog.MessageContainer.Text = message;
            dialog.OkButton.Visibility = Visibility.Visible;
            dialog.ShowDialog();
            return MessageBoxResult.OK;
        }

        public static MessageBoxResult ShowError(string message, string title = "Ошибка")
        {
            var dialog = new CustomMessageBox() { Title = title };
            dialog.MessageContainer.Text = message;
            dialog.MessageContainer.Foreground = dialog.FindResource("ErrorBrush") as SolidColorBrush;
            dialog.OkButton.Visibility = Visibility.Visible;
            dialog.ShowDialog();
            return MessageBoxResult.OK;
        }

        public static MessageBoxResult ShowYesNo(string message, string title = "Подтверждение")
        {
            var dialog = new CustomMessageBox() { Title = title };
            dialog.MessageContainer.Text = message;

            dialog.YesButton.Visibility = Visibility.Visible;
            dialog.NoButton.Visibility = Visibility.Visible;

            dialog.OkButton.Visibility = Visibility.Collapsed;

            dialog.ShowDialog();
            return dialog.Result;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.OK;
            this.DialogResult = true;
            this.Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.Yes;
            this.DialogResult = true;
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.No;
            this.DialogResult = false;
            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.No;
            this.DialogResult = false;
            this.Close();
        }
    }
}