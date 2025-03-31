using System;
using System.Windows;
using System.Windows.Controls;
using TechFlow.Models;

namespace TechFlow.Windows
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        UserFromDb userFromDb = new UserFromDb();
        public Registration()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Authorization loginWindow = new Authorization();
            loginWindow.Show();

            this.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            bool isEmployee = EmployeeRadioButton.IsChecked == true;

            // Обязательные поля для всех
            if (string.IsNullOrWhiteSpace(LoginField.Text) ||
                string.IsNullOrWhiteSpace(NameField.Text) ||
                string.IsNullOrWhiteSpace(LastNameField.Text) ||
                string.IsNullOrWhiteSpace(EmailField.Text) ||
                string.IsNullOrWhiteSpace(PasswordField.Password) ||
                string.IsNullOrWhiteSpace(PasswordFieldConfirm.Password))
            {
                CustomMessageBox.Show("Необходимо заполнить все обязательные поля!");
                return;
            }

            // Дополнительное поле для заказчика
            if (!isEmployee && string.IsNullOrWhiteSpace(OrganizationField.Text))
            {
                CustomMessageBox.Show("Для заказчика необходимо указать название организации!");
                return;
            }

            // Проверка совпадения паролей
            if (!userFromDb.CheckPassword(PasswordField.Password, PasswordFieldConfirm.Password))
            {
                return;
            }

            try
            {
                // Проверка уникальности логина во всех таблицах
                if (userFromDb.IsLoginExists(LoginField.Text))
                {
                    CustomMessageBox.Show("Этот логин уже занят!");
                    return;
                }

                // Проверка уникальности email во всех таблицах
                if (userFromDb.IsEmailExists(EmailField.Text))
                {
                    CustomMessageBox.Show("Этот email уже зарегистрирован!");
                    return;
                }

                if (isEmployee)
                {
                    // Регистрация сотрудника
                    int employeeId = userFromDb.AddEmployee(
                        LoginField.Text,
                        NameField.Text,
                        LastNameField.Text,
                        EmailField.Text,
                        PasswordField.Password,
                        null, // phone
                        null, // address
                        null, // date_of_birth
                        -1     // роль "Не определено" по умолчанию
                    );

                    if (employeeId > 0)
                    {
                        // Теперь устанавливаем роль "Сотрудник"
                        if (userFromDb.SetEmployeeRole(employeeId))
                        {
                            CustomMessageBox.Show("Сотрудник успешно зарегистрирован!");
                            this.Close();
                        }
                        else
                        {
                            // Если не удалось установить роль, можно удалить сотрудника или оставить с role_id = -1
                            CustomMessageBox.Show("Сотрудник зарегистрирован, но не удалось установить роль!");
                        }
                    }
                }
                else
                {
                    // Регистрация клиента
                    if (userFromDb.RegisterClient(
                        LoginField.Text,
                        NameField.Text,
                        LastNameField.Text,
                        EmailField.Text,
                        null, // phone
                        OrganizationField.Text,
                        PasswordField.Password))
                    {
                        CustomMessageBox.Show("Клиент успешно зарегистрирован!");
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка регистрации: {ex.Message}");
            }
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

        private void AccountType_Checked(object sender, RoutedEventArgs e)
        {
            if (OrganizationLabel == null || OrganizationField == null)
            {
                return;
            }

            bool isEmployee = EmployeeRadioButton.IsChecked == true;

            // Поле организации только для заказчиков
            OrganizationLabel.Visibility = isEmployee ? Visibility.Collapsed : Visibility.Visible;
            OrganizationField.Visibility = isEmployee ? Visibility.Collapsed : Visibility.Visible;

            // Логин и пароли видны всегда (убрали их из условия видимости)
        }
    }
}