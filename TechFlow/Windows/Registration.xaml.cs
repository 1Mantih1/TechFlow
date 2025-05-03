using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using TechFlow.Classes;
using TechFlow.Models;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace TechFlow.Windows
{
    public partial class Registration : Window
    {
        private UserFromDb userFromDb = new UserFromDb();
        private EmailSender emailSender = new EmailSender();
        private string generatedVerificationCode;
        private string userEmail;
        private string tempPassword;

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

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
                return;

            bool isEmployee = EmployeeRadioButton.IsChecked == true;
            userEmail = EmailField.Text;
            tempPassword = PasswordField.Password;

            try
            {
                generatedVerificationCode = GenerateVerificationCode();
                await System.Threading.Tasks.Task.Run(() => emailSender.SendVerificationCode(userEmail, generatedVerificationCode));
                SwitchToVerificationPanel();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка отправки кода подтверждения: {ex.Message}");
            }
        }

        private bool ValidateFields()
        {
            ClearAllErrors();
            bool isValid = true;
            bool isEmployee = EmployeeRadioButton.IsChecked == true;

            if (string.IsNullOrWhiteSpace(LoginField.Text))
            {
                ShowError("Введите логин", LoginError);
                isValid = false;
            }
            else if (LoginField.Text.Length < 4)
            {
                ShowError("Логин должен содержать минимум 4 символа", LoginError);
                isValid = false;
            }
            else if (userFromDb.IsLoginExists(LoginField.Text))
            {
                ShowError("Этот логин уже занят", LoginError);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(NameField.Text))
            {
                ShowError("Введите имя", NameError);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(LastNameField.Text))
            {
                ShowError("Введите фамилию", LastNameError);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(EmailField.Text))
            {
                ShowError("Введите email", EmailError);
                isValid = false;
            }
            else if (!IsValidEmail(EmailField.Text))
            {
                ShowError("Некорректный email", EmailError);
                isValid = false;
            }
            else if (userFromDb.IsEmailExists(EmailField.Text))
            {
                ShowError("Этот email уже зарегистрирован", EmailError);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(PasswordField.Password))
            {
                ShowError("Введите пароль", PasswordError);
                isValid = false;
            }
            else if (PasswordField.Password.Length < 8)
            {
                ShowError("Пароль должен содержать минимум 8 символов", PasswordError);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(PasswordFieldConfirm.Password))
            {
                ShowError("Подтвердите пароль", PasswordConfirmError);
                isValid = false;
            }
            else if (PasswordField.Password != PasswordFieldConfirm.Password)
            {
                ShowError("Пароли не совпадают", PasswordConfirmError);
                isValid = false;
            }

            if (!isEmployee && string.IsNullOrWhiteSpace(OrganizationField.Text))
            {
                ShowError("Введите название организации", OrganizationError);
                isValid = false;
            }

            return isValid;
        }

        private void ShowError(string message, TextBlock errorControl)
        {
            if (errorControl == null) return;

            errorControl.Text = message;
            errorControl.Visibility = Visibility.Visible;
        }

        private void ClearAllErrors()
        {
            if (LoginError != null) LoginError.Visibility = Visibility.Collapsed;
            if (NameError != null) NameError.Visibility = Visibility.Collapsed;
            if (LastNameError != null) LastNameError.Visibility = Visibility.Collapsed;
            if (EmailError != null) EmailError.Visibility = Visibility.Collapsed;
            if (PasswordError != null) PasswordError.Visibility = Visibility.Collapsed;
            if (PasswordConfirmError != null) PasswordConfirmError.Visibility = Visibility.Collapsed;
            if (OrganizationError != null) OrganizationError.Visibility = Visibility.Collapsed;
            if (VerificationError != null) VerificationError.Visibility = Visibility.Collapsed;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private void SwitchToVerificationPanel()
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            fadeOut.Completed += (s, e) =>
            {
                RegistrationLayout.Visibility = Visibility.Collapsed;
                VerificationLayout.Visibility = Visibility.Visible;

                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                VerificationPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            RegistrationForm.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(VerificationCodeField.Text))
            {
                VerificationError.Text = "Введите код подтверждения";
                VerificationError.Visibility = Visibility.Visible;
                return;
            }

            if (VerificationCodeField.Text != generatedVerificationCode)
            {
                VerificationError.Text = "Неверный код подтверждения";
                VerificationError.Visibility = Visibility.Visible;
                return;
            }

            VerificationError.Visibility = Visibility.Collapsed;
            CompleteRegistration();
        }

        private void CompleteRegistration()
        {
            try
            {
                bool success = false;
                bool isEmployee = EmployeeRadioButton.IsChecked == true;

                if (isEmployee)
                {
                    int employeeId = userFromDb.AddEmployee(
                        LoginField.Text,
                        NameField.Text,
                        LastNameField.Text,
                        EmailField.Text,
                        tempPassword,
                        null, null, null, 34);

                    if (employeeId > 0 && userFromDb.SetEmployeeRole(employeeId))
                    {
                        success = true;
                    }
                }
                else
                {
                    success = userFromDb.RegisterClient(
                        LoginField.Text,
                        NameField.Text,
                        LastNameField.Text,
                        EmailField.Text,
                        null,
                        OrganizationField.Text,
                        tempPassword);
                }

                if (success)
                {
                    CustomMessageBox.Show("Регистрация успешно завершена!");
                    this.Close();
                }
                else
                {
                    CustomMessageBox.Show("Ошибка регистрации. Попробуйте снова.");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при регистрации: {ex.Message}");
            }
        }

        private async void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                generatedVerificationCode = GenerateVerificationCode();
                await System.Threading.Tasks.Task.Run(() => emailSender.SendVerificationCode(userEmail, generatedVerificationCode));
                CustomMessageBox.Show("Новый код подтверждения отправлен!");
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при отправке кода: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            fadeOut.Completed += (s, args) =>
            {
                VerificationLayout.Visibility = Visibility.Collapsed;
                RegistrationLayout.Visibility = Visibility.Visible;

                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                RegistrationForm.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            VerificationPanel.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void VerificationCodeField_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void VerificationCodeField_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Можно добавить логику для автоматического перехода между полями
        }

        private void AccountType_Checked(object sender, RoutedEventArgs e)
        {
            ClearAllErrors(); // Очищаем ошибки при смене типа аккаунта
            bool isEmployeeChecked = EmployeeRadioButton.IsChecked == true;
            AnimateOrganizationFields(!isEmployeeChecked);
        }

        private void AnimateOrganizationFields(bool show)
        {
            if (OrganizationLabel == null || OrganizationField == null)
                return;

            if (show)
            {
                OrganizationLabel.Visibility = Visibility.Visible;
                OrganizationField.Visibility = Visibility.Visible;

                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                OrganizationLabel.BeginAnimation(UIElement.OpacityProperty, animation);
                OrganizationField.BeginAnimation(UIElement.OpacityProperty, animation);
            }
            else
            {
                var animation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                animation.Completed += (s, args) =>
                {
                    if (OrganizationLabel != null) OrganizationLabel.Visibility = Visibility.Collapsed;
                    if (OrganizationField != null) OrganizationField.Visibility = Visibility.Collapsed;
                };

                OrganizationLabel.BeginAnimation(UIElement.OpacityProperty, animation);
                OrganizationField.BeginAnimation(UIElement.OpacityProperty, animation);
            }
        }

        private void PasswordField_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            PasswordBoxHelper.SetPasswordLength(passwordBox, passwordBox.Password.Length);

            PasswordStrengthGrid.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                ? Visibility.Collapsed
                : Visibility.Visible;

            UpdatePasswordStrength(passwordBox.Password);
        }

        private void PasswordFieldConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            PasswordBoxHelper.SetPasswordLength(passwordBox, passwordBox.Password.Length);

            if (!string.IsNullOrEmpty(passwordBox.Password) &&
                PasswordField.Password != passwordBox.Password)
            {
                ShowError("Пароли не совпадают", PasswordConfirmError);
            }
            else
            {
                PasswordConfirmError.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdatePasswordStrength(string password)
        {
            int strength = CalculatePasswordStrength(password);
            Color strengthColor = GetStrengthColor(strength);

            // Пропорционально заполняем ширину индикатора
            double fillWidth = 320 * (strength / 4.0);  // Если максимальная сила = 4, ширина будет 350

            // Убедитесь, что ширина индикатора не выходит за пределы
            fillWidth = Math.Min(fillWidth, 320);  // Максимальная ширина 350

            PasswordStrengthFill.Background = new SolidColorBrush(strengthColor);
            PasswordStrengthFill.Width = fillWidth;

            PasswordStrengthText.Text = GetStrengthText(strength);
            PasswordStrengthText.Foreground = new SolidColorBrush(strengthColor);
        }


        private int CalculatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password)) return 0;

            int strength = 0;
            if (password.Length >= 8) strength++;
            if (Regex.IsMatch(password, "[A-Z]")) strength++;
            if (Regex.IsMatch(password, "[a-z]")) strength++;
            if (Regex.IsMatch(password, "[0-9]")) strength++;
            if (Regex.IsMatch(password, "[^a-zA-Z0-9]")) strength++;

            return Math.Min(strength, 4);
        }

        private Color GetStrengthColor(int strength)
        {
            // Защита от некорректных значений
            if (strength <= 0) return Colors.Gray;

            if (strength == 1)
                return Color.FromRgb(0xFF, 0x64, 0x64); // Weak — красный
            else if (strength == 2)
                return Color.FromRgb(0xFF, 0xC8, 0x64); // Medium — оранжевый
            else if (strength == 3)
                return Color.FromRgb(0x64, 0xC8, 0x64); // Good — светло-зелёный
            else if (strength >= 4)
                return Color.FromRgb(0x32, 0xA5, 0x32); // Strong — насыщенный зелёный

            // На случай непредвиденного значения
            return Colors.Gray;
        }


        private string GetStrengthText(int strength)
        {
            if (strength == 1) return "Слабый";
            else if (strength == 2) return "Средний";
            else if (strength == 3) return "Хороший";
            else if (strength == 4) return "Надежный";
            else return "";
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