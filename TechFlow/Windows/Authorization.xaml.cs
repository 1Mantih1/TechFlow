using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Windows
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        UserFromDb userFromDb = new UserFromDb();
        public static event Action<User> OnUserUpdated;
        public static User currentUser { get; set; } = null;

        public Authorization()
        {
            InitializeComponent();
        }

        public static void NotifyUserUpdated(User updatedUser)
        {
            OnUserUpdated?.Invoke(updatedUser);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Registration registrationWindow = new Registration();
            registrationWindow.Show();

            this.Close();
        }

        private void LoginButtonMain_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailorLoginField.Text) || string.IsNullOrWhiteSpace(PasswordField.Password))
            {
                MessageBox.Show("Введите данные");
                return;
            }

            var authenticatedUser = AuthenticateUser(EmailorLoginField.Text, PasswordField.Password);

            if (authenticatedUser is User user)
            {
                currentUser = user;

                if (userFromDb.IsPendingEmployee(user.UserId))
                {
                    MessageBox.Show("Ваш аккаунт ожидает подтверждения администратором");
                    AccountConfirmationWaiting accountConfirmationWaiting = new AccountConfirmationWaiting();
                    accountConfirmationWaiting.Show();
                    this.Close();
                }
                else
                {
                    ProjectManagement projectManagementWindow = new ProjectManagement();
                    projectManagementWindow.Show();
                    this.Close();
                }
            }
            else if (authenticatedUser is Client client)
            {
                // Логика для клиента
                MessageBox.Show("папапапапа");
                CreateProjectOrderWindow createProjectOrderWindow = new CreateProjectOrderWindow();
                createProjectOrderWindow.Show();
                this.Close();

            }
            else
            {
                MessageBox.Show("Неверные данные или пользователь не найден");
            }
        }

        public object AuthenticateUser(string loginOrEmail, string password)
        {
            // Сначала проверяем сотрудника
            var user = userFromDb.GetUser(loginOrEmail, password);
            if (user != null) return user;

            // Затем проверяем клиента
            var client = userFromDb.GetClient(loginOrEmail, password);
            if (client != null) return client;

            return null;
        }


        private void PasswordField_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                var placeholderText = passwordBox.Template.FindName("PlaceholderText", passwordBox) as TextBlock;
                if (placeholderText != null)
                {
                    placeholderText.Visibility = string.IsNullOrEmpty(passwordBox.Password) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void TogglePasswordVisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordField.Visibility == Visibility.Visible)
            {
                PasswordField.Visibility = Visibility.Collapsed;
                VisiblePasswordField.Visibility = Visibility.Visible;
                VisiblePasswordField.Text = PasswordField.Password;
                TogglePasswordVisibilityButton.Content = new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff };
            }
            else
            {
                PasswordField.Visibility = Visibility.Visible;
                VisiblePasswordField.Visibility = Visibility.Collapsed;
                PasswordField.Password = VisiblePasswordField.Text;
                TogglePasswordVisibilityButton.Content = new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye };
            }
        }
    }
}
