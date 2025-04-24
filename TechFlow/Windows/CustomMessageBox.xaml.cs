using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TechFlow.Windows
{
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox()
        {
            InitializeComponent();
        }

        public static MessageBoxResult Show(string message, string title = "Сообщение")
        {
            var dialog = new CustomMessageBox() { Title = title };
            dialog.MessageContainer.Text = message;
            dialog.ShowDialog();
            return MessageBoxResult.OK;
        }

        public static MessageBoxResult ShowError(string message, string title = "Ошибка")
        {
            var dialog = new CustomMessageBox() { Title = title };
            dialog.MessageContainer.Text = message;
            dialog.MessageContainer.Foreground = dialog.FindResource("ErrorBrush") as SolidColorBrush;
            dialog.ShowDialog();
            return MessageBoxResult.OK;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        // Перетаскивание окна за кастомный заголовок
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        // Закрытие окна
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}