using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TechFlow.Windows;

namespace TechFlow.Windows
{
    /// <summary>
    /// Логика взаимодействия для AccountConfirmationWaiting.xaml
    /// </summary>
    public partial class AccountConfirmationWaiting : Window
    {
        public AccountConfirmationWaiting()
        {
            InitializeComponent();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика выхода из системы
            Authorization authorization = new Authorization();
            authorization.Show();
            this.Close();
        }

        private void ResendRequestButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика повторной отправки запроса на подтверждение
            MessageBox.Show("Запрос на подтверждение аккаунта отправлен повторно", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
