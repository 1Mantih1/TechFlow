using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;
using System.IO;
using System.Text.RegularExpressions;

namespace TechFlow.Pages
{
    public partial class EditProfilePage : Page
    {
        UserFromDb userFromDb = new UserFromDb();
        private bool _isFormValid = true;

        public EditProfilePage()
        {
            InitializeComponent();
            LoadUserData();
            InitializeValidation();
        }

        private void InitializeValidation()
        {
            // Убрали привязку валидации к LostFocus, оставим только при сохранении
        }

        public void LoadUserData()
        {
            try
            {
                User user = Authorization.currentUser;

                if (user == null)
                {
                    MessageBox.Show("Не удалось загрузить данные пользователя.");
                    return;
                }

                NameField.Text = user.FirstName;
                LastNameField.Text = user.LastName;
                PhoneField.Text = user.Phone;
                EmailField.Text = user.Email;
                AddressField.Text = user.Address;
                DOBField.SelectedDate = user.DateOfBirth;

                LoadAvatar(user.ImagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadAvatar(string imagePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    SetDefaultAvatar();
                    return;
                }

                if (Path.IsPathRooted(imagePath))
                {
                    if (File.Exists(imagePath))
                    {
                        SetAvatarImage(imagePath);
                        return;
                    }
                }
                else if (imagePath.StartsWith("pack:"))
                {
                    try
                    {
                        AvatarImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                        return;
                    }
                    catch { }
                }

                string[] possibleBasePaths = {
                    AppDomain.CurrentDomain.BaseDirectory,
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", ".."),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..")
                };

                foreach (var basePath in possibleBasePaths)
                {
                    try
                    {
                        string fullPath = Path.GetFullPath(Path.Combine(basePath, imagePath));
                        if (File.Exists(fullPath))
                        {
                            SetAvatarImage(fullPath);
                            return;
                        }
                    }
                    catch { }
                }

                SetDefaultAvatar();
                CustomMessageBox.Show($"Не удалось загрузить аватар по пути: {imagePath}");
            }
            catch (Exception ex)
            {
                SetDefaultAvatar();
                Console.WriteLine($"Ошибка загрузки аватара: {ex.Message}");
            }
        }

        private void SetAvatarImage(string imagePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(imagePath);
            bitmap.EndInit();
            bitmap.Freeze();
            AvatarImage.Source = bitmap;
        }

        private void SetDefaultAvatar()
        {
            string[] defaultPaths = {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "avatar", "man1.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "avatar", "man1.png"),
                "pack://application:,,,/TechFlow;component/Resources/man1.png"
            };

            foreach (var path in defaultPaths)
            {
                try
                {
                    if (path.StartsWith("pack:") || File.Exists(path))
                    {
                        AvatarImage.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                        return;
                    }
                }
                catch { }
            }

            AvatarImage.Source = null;
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            _isFormValid = true;

            ValidateNameField();
            ValidateLastNameField();
            ValidatePhoneField();
            ValidateEmailField();
            ValidateAddressField();
            ValidateDOBField();

            if (!_isFormValid)
            {
                return;
            }

            try
            {
                User updatedUser = new User
                {
                    UserId = Authorization.currentUser.UserId,
                    Login = Authorization.currentUser.Login,
                    FirstName = NameField.Text.Trim(),
                    LastName = LastNameField.Text.Trim(),
                    Phone = PhoneField.Text.Trim(),
                    Email = EmailField.Text.Trim(),
                    Address = AddressField.Text.Trim(),
                    DateOfBirth = DOBField.SelectedDate.Value,
                    ImagePath = Authorization.currentUser.ImagePath
                };

                if (userFromDb.UpdateEmployeeProfile(updatedUser))
                {
                    Authorization.currentUser = updatedUser;
                    Authorization.NotifyUserUpdated(updatedUser);
                    MessageBox.Show("Профиль успешно обновлен.");
                }
                else
                {
                    MessageBox.Show("Ошибка при сохранении изменений.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void ValidateNameField()
        {
            string name = NameField.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Имя обязательно для заполнения");
                _isFormValid = false;
                return;
            }

            if (name.Length < 2 || name.Length > 50)
            {
                MessageBox.Show("Имя должно быть от 2 до 50 символов");
                _isFormValid = false;
                return;
            }
        }

        private void ValidateLastNameField()
        {
            string lastName = LastNameField.Text.Trim();

            if (string.IsNullOrEmpty(lastName))
            {
                MessageBox.Show("Фамилия обязательна для заполнения");
                _isFormValid = false;
                return;
            }

            if (lastName.Length < 2 || lastName.Length > 50)
            {
                MessageBox.Show("Фамилия должна быть от 2 до 50 символов");
                _isFormValid = false;
                return;
            }
        }

        private void ValidatePhoneField()
        {
            string phone = PhoneField.Text.Trim();

            if (string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Телефон обязателен для заполнения");
                _isFormValid = false;
                return;
            }

            string digitsOnly = Regex.Replace(phone, @"[^\d]", "");

            if (digitsOnly.Length < 10 || digitsOnly.Length > 15)
            {
                MessageBox.Show("Телефон должен содержать от 10 до 15 цифр");
                _isFormValid = false;
                return;
            }
        }

        private void ValidateEmailField()
        {
            string email = EmailField.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Email обязателен для заполнения");
                _isFormValid = false;
                return;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                {
                    MessageBox.Show("Некорректный email");
                    _isFormValid = false;
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Некорректный email");
                _isFormValid = false;
                return;
            }

            if (email.Length > 100)
            {
                MessageBox.Show("Email должен быть не длиннее 100 символов");
                _isFormValid = false;
                return;
            }
        }

        private void ValidateAddressField()
        {
            string address = AddressField.Text.Trim();

            if (string.IsNullOrEmpty(address))
            {
                MessageBox.Show("Адрес обязателен для заполнения");
                _isFormValid = false;
                return;
            }

            if (address.Length < 5 || address.Length > 200)
            {
                MessageBox.Show("Адрес должен быть от 5 до 200 символов");
                _isFormValid = false;
                return;
            }
        }

        private void ValidateDOBField()
        {
            if (!DOBField.SelectedDate.HasValue)
            {
                MessageBox.Show("Дата рождения обязательна для заполнения");
                _isFormValid = false;
                return;
            }

            DateTime dob = DOBField.SelectedDate.Value;
            DateTime minDate = new DateTime(1900, 1, 1);
            DateTime maxDate = DateTime.Today.AddYears(-14);

            if (dob < minDate || dob > maxDate)
            {
                MessageBox.Show($"Дата рождения должна быть между {minDate:dd.MM.yyyy} и {maxDate:dd.MM.yyyy}");
                _isFormValid = false;
                return;
            }
        }

        private void ChangeAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            if (AvatarFrame.Visibility == Visibility.Visible)
            {
                AvatarFrame.Visibility = Visibility.Collapsed;
                ProfileGrid.Visibility = Visibility.Visible;
            }
            else
            {
                ProfileGrid.Visibility = Visibility.Collapsed;
                AvatarFrame.Visibility = Visibility.Visible;
                AvatarFrame.Navigate(new AvatarsPage(this));
            }
        }

        public void UpdateAvatar(string newAvatarPath)
        {
            try
            {
                if (string.IsNullOrEmpty(newAvatarPath))
                {
                    SetDefaultAvatar();
                    return;
                }

                if (Uri.IsWellFormedUriString(newAvatarPath, UriKind.Absolute))
                {
                    AvatarImage.Source = new BitmapImage(new Uri(newAvatarPath));
                }
                else if (File.Exists(newAvatarPath))
                {
                    newAvatarPath = new Uri(newAvatarPath).AbsoluteUri;
                    AvatarImage.Source = new BitmapImage(new Uri(newAvatarPath));
                }
                else
                {
                    SetDefaultAvatar();
                    return;
                }

                Authorization.currentUser.ImagePath = newAvatarPath;
                userFromDb.AddOrUpdateImagePath(Authorization.currentUser.UserId, newAvatarPath);
            }
            catch (Exception ex)
            {
                SetDefaultAvatar();
                MessageBox.Show($"Ошибка при обновлении аватара: {ex.Message}");
            }
        }
    }
}