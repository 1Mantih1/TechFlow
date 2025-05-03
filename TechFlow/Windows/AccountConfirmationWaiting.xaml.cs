using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TechFlow.Windows
{
    public partial class AccountConfirmationWaiting : Window
    {
        public AccountConfirmationWaiting()
        {
            InitializeComponent();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Authorization authorization = new Authorization();
            authorization.Show();
            this.Close();
        }

        private void ResendRequestButton_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.Show("Запрос на подтверждение аккаунта отправлен повторно", "Информация");
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                ((PackIcon)((Button)sender).Content).Kind = PackIconKind.WindowMaximize;
            }
            else
            {
                WindowState = WindowState.Maximized;
                ((PackIcon)((Button)sender).Content).Kind = PackIconKind.WindowRestore;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WindowHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateWindowCorners();
        }

        private void UpdateWindowCorners()
        {
            double radius = (WindowState == WindowState.Maximized) ? 0 : 16;

            MainBorder.CornerRadius = new CornerRadius(radius);

            this.Clip = new RectangleGeometry(
                new Rect(0, 0, ActualWidth, ActualHeight),
                radius,
                radius
            );
        }
    }
}
