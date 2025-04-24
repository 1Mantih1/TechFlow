using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MaterialDesignThemes.Wpf;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Windows
{
    public partial class Authorization : Window
    {
        private UserFromDb userFromDb = new UserFromDb();
        private EmailSender emailSender = new EmailSender();
        private string generatedVerificationCode;
        private string recoveryEmail;

        public static event Action<User> OnUserUpdated;
        public static User currentUser { get; set; } = null;
        public static Client currentClient { get; set; } = null;

        public Authorization()
        {
            InitializeComponent();
            Loaded += OnWindowLoaded;

            NewPasswordField.PasswordChanged += NewPasswordField_PasswordChanged;
            ConfirmPasswordField.PasswordChanged += ConfirmPasswordField_PasswordChanged;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            LoginForm.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        public static void NotifyUserUpdated(User updatedUser)
        {
            OnUserUpdated?.Invoke(updatedUser);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = new Registration();
            registrationWindow.Show();
            this.Close();
        }

        private void LoginButtonMain_Click(object sender, RoutedEventArgs e)
        {
            ClearAllErrors();

            if (string.IsNullOrWhiteSpace(EmailorLoginField.Text))
            {
                ShowError("Введите логин или email", LoginError);
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordField.Password))
            {
                ShowError("Введите пароль", PasswordError);
                return;
            }

            var password = PasswordField.Visibility == Visibility.Visible ?
                          PasswordField.Password :
                          VisiblePasswordField.Text;

            var authenticatedUser = AuthenticateUser(EmailorLoginField.Text, password);

            if (authenticatedUser is User user)
            {
                currentUser = user;

                if (userFromDb.IsPendingEmployee(user.UserId))
                {
                    CustomMessageBox.Show("Ваш аккаунт ожидает подтверждения администратором");
                    var accountConfirmationWaiting = new AccountConfirmationWaiting();
                    accountConfirmationWaiting.Show();
                    this.Close();
                }
                else
                {
                    var fadeOut = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.3)
                    };

                    fadeOut.Completed += (s, args) =>
                    {
                        var projectManagementWindow = new ProjectManagement();
                        projectManagementWindow.Show();
                        this.Close();
                    };

                    LoginForm.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                }
            }
            else if (authenticatedUser is Client client)
            {
                currentClient = client;

                var fadeOut = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                fadeOut.Completed += (s, args) =>
                {
                    var createProjectOrderWindow = new CreateProjectOrderWindow();
                    createProjectOrderWindow.Show();
                    this.Close();
                };

                LoginForm.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
            else
            {
                ShowError("Неверные данные или пользователь не найден", PasswordError);

                var shake = new DoubleAnimationUsingKeyFrames();
                shake.KeyFrames.Add(new LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(0)));
                shake.KeyFrames.Add(new LinearDoubleKeyFrame(10, TimeSpan.FromSeconds(0.05)));
                shake.KeyFrames.Add(new LinearDoubleKeyFrame(-10, TimeSpan.FromSeconds(0.1)));
                shake.KeyFrames.Add(new LinearDoubleKeyFrame(10, TimeSpan.FromSeconds(0.15)));
                shake.KeyFrames.Add(new LinearDoubleKeyFrame(-10, TimeSpan.FromSeconds(0.2)));
                shake.KeyFrames.Add(new LinearDoubleKeyFrame(10, TimeSpan.FromSeconds(0.25)));
                shake.KeyFrames.Add(new LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(0.3)));

                var transform = new TranslateTransform();
                LoginForm.RenderTransform = transform;
                transform.BeginAnimation(TranslateTransform.XProperty, shake);
            }
        }

        public object AuthenticateUser(string loginOrEmail, string password)
        {
            var user = userFromDb.GetUser(loginOrEmail, password);
            if (user != null) return user;

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
                    placeholderText.Visibility = string.IsNullOrEmpty(passwordBox.Password) ?
                                                Visibility.Visible :
                                                Visibility.Collapsed;
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
                (TogglePasswordVisibilityButton.Content as PackIcon).Kind = PackIconKind.EyeOff;
                VisiblePasswordField.Focus();
                VisiblePasswordField.CaretIndex = VisiblePasswordField.Text.Length;
            }
            else
            {
                PasswordField.Visibility = Visibility.Visible;
                VisiblePasswordField.Visibility = Visibility.Collapsed;
                PasswordField.Password = VisiblePasswordField.Text;
                (TogglePasswordVisibilityButton.Content as PackIcon).Kind = PackIconKind.Eye;
                PasswordField.Focus();
            }
        }

        private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAllErrors();
            ClearRecoveryForms();

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            fadeOut.Completed += (s, args) =>
            {
                MainGrid.Visibility = Visibility.Collapsed;
                RecoveryLayout.Visibility = Visibility.Visible;

                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                RecoveryPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            LoginForm.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private async void SendRecoveryCode_Click(object sender, RoutedEventArgs e)
        {
            ClearAllErrors();

            if (string.IsNullOrWhiteSpace(RecoveryEmailField.Text))
            {
                ShowError("Введите email", RecoveryError);
                return;
            }

            if (!IsValidEmail(RecoveryEmailField.Text))
            {
                ShowError("Некорректный email", RecoveryError);
                return;
            }

            try
            {
                if (!userFromDb.IsEmailExists(RecoveryEmailField.Text))
                {
                    ShowError("Email не зарегистрирован", RecoveryError);
                    return;
                }

                recoveryEmail = RecoveryEmailField.Text;
                generatedVerificationCode = GenerateVerificationCode();

                SendRecoveryCodeButton.IsEnabled = false;
                SendRecoveryCodeButton.Content = "Отправка...";

                await Task.Delay(100);

                bool sendResult = await Task.Run(() =>
                    emailSender.SendPasswordRecoveryCode(recoveryEmail, generatedVerificationCode));

                if (!sendResult)
                {
                    ShowError("Ошибка при отправке кода", RecoveryError);
                    return;
                }

                SwitchToVerificationLayout();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка отправки кода: {ex.Message}", RecoveryError);
            }
            finally
            {
                SendRecoveryCodeButton.IsEnabled = true;
                SendRecoveryCodeButton.Content = "Отправить код";
            }
        }

        private void NewPasswordField_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                var placeholderText = passwordBox.Template.FindName("PlaceholderText", passwordBox) as TextBlock;
                if (placeholderText != null)
                {
                    placeholderText.Visibility = string.IsNullOrEmpty(passwordBox.Password) ?
                                                Visibility.Visible :
                                                Visibility.Collapsed;
                }
            }
        }

        private void ConfirmPasswordField_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                var placeholderText = passwordBox.Template.FindName("PlaceholderText", passwordBox) as TextBlock;
                if (placeholderText != null)
                {
                    placeholderText.Visibility = string.IsNullOrEmpty(passwordBox.Password) ?
                                                Visibility.Visible :
                                                Visibility.Collapsed;
                }
            }
        }

        private void BackFromRecovery_Click(object sender, RoutedEventArgs e)
        {
            ClearAllErrors();
            ClearRecoveryForms();

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            fadeOut.Completed += (s, args) =>
            {
                RecoveryLayout.Visibility = Visibility.Collapsed;
                MainGrid.Visibility = Visibility.Visible;

                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                LoginForm.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            RecoveryPanel.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void SwitchToVerificationLayout()
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            fadeOut.Completed += (s, args) =>
            {
                RecoveryLayout.Visibility = Visibility.Collapsed;
                VerificationLayout.Visibility = Visibility.Visible;
                VerificationCodeField.Text = "";
                VerificationError.Visibility = Visibility.Collapsed;

                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                VerificationPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            RecoveryPanel.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAllErrors();

            if (string.IsNullOrWhiteSpace(VerificationCodeField.Text))
            {
                ShowError("Введите код подтверждения", VerificationError);
                return;
            }

            if (VerificationCodeField.Text != generatedVerificationCode)
            {
                ShowError("Неверный код подтверждения", VerificationError);
                return;
            }

            VerificationError.Visibility = Visibility.Collapsed;
            SwitchToPasswordResetLayout();
        }

        private async void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAllErrors();

            try
            {
                generatedVerificationCode = GenerateVerificationCode();

                ResendButton.IsEnabled = false;
                ResendButton.Content = "Отправка...";

 
                await Task.Delay(100);

                bool sendResult = await Task.Run(() =>
                    emailSender.SendPasswordRecoveryCode(recoveryEmail, generatedVerificationCode));

                if (!sendResult)
                {
                    ShowError("Ошибка при отправке кода", VerificationError);
                    return;
                }

                CustomMessageBox.Show("Новый код подтверждения отправлен на ваш email");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка отправки кода: {ex.Message}", VerificationError);
            }
            finally
            {
                ResendButton.IsEnabled = true;
                ResendButton.Content = "Отправить код повторно";
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAllErrors();
            ClearRecoveryForms();

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            fadeOut.Completed += (s, args) =>
            {
                VerificationLayout.Visibility = Visibility.Collapsed;
                RecoveryLayout.Visibility = Visibility.Visible;

                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                RecoveryPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            VerificationPanel.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void SwitchToPasswordResetLayout()
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
                PasswordResetLayout.Visibility = Visibility.Visible;
                NewPasswordField.Password = "";
                ConfirmPasswordField.Password = "";
                PasswordResetError.Visibility = Visibility.Collapsed;

                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                PasswordResetPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            VerificationPanel.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAllErrors();

            if (string.IsNullOrWhiteSpace(NewPasswordField.Password))
            {
                ShowError("Введите новый пароль", PasswordResetError);
                return;
            }

            if (NewPasswordField.Password.Length < 8)
            {
                ShowError("Пароль должен содержать минимум 8 символов", PasswordResetError);
                return;
            }

            if (NewPasswordField.Password != ConfirmPasswordField.Password)
            {
                ShowError("Пароли не совпадают", PasswordResetError);
                return;
            }

            try
            {
                bool success = userFromDb.UpdatePassword(recoveryEmail, NewPasswordField.Password);

                if (success)
                {
                    CustomMessageBox.Show("Пароль успешно изменен. Теперь вы можете войти с новым паролем.");
                    ClearRecoveryForms();

                    var fadeOut = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.3)
                    };

                    fadeOut.Completed += (s, args) =>
                    {
                        PasswordResetLayout.Visibility = Visibility.Collapsed;
                        MainGrid.Visibility = Visibility.Visible;

                        var fadeIn = new DoubleAnimation
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.3)
                        };

                        LoginForm.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                    };

                    PasswordResetPanel.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                }
                else
                {
                    ShowError("Ошибка при изменении пароля", PasswordResetError);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}", PasswordResetError);
            }
        }

        private void BackFromPasswordReset_Click(object sender, RoutedEventArgs e)
        {
            ClearAllErrors();
            ClearRecoveryForms();

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            fadeOut.Completed += (s, args) =>
            {
                PasswordResetLayout.Visibility = Visibility.Collapsed;
                VerificationLayout.Visibility = Visibility.Visible;

                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                VerificationPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            PasswordResetPanel.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void VerificationCodeField_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void VerificationCodeField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VerificationCodeField.Text.Length == 6)
            {
                VerifyButton_Click(null, null);
            }
        }

        private void ShowError(string message, TextBlock errorControl)
        {
            if (errorControl == null) return;
            errorControl.Text = message;
            errorControl.Visibility = Visibility.Visible;
        }

        private void ClearAllErrors()
        {
            LoginError.Visibility = Visibility.Collapsed;
            PasswordError.Visibility = Visibility.Collapsed;
            RecoveryError.Visibility = Visibility.Collapsed;
            VerificationError.Visibility = Visibility.Collapsed;
            PasswordResetError.Visibility = Visibility.Collapsed;
        }

        private void ClearRecoveryForms()
        {
            RecoveryEmailField.Text = "";
            VerificationCodeField.Text = "";
            NewPasswordField.Password = "";
            ConfirmPasswordField.Password = "";
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