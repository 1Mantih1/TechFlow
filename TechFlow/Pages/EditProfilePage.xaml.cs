using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;
using System.IO;

namespace TechFlow.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditProfilePage.xaml
    /// </summary>
    public partial class EditProfilePage : Page
    {
        UserFromDb userFromDb = new UserFromDb();

        public EditProfilePage()
        {
            InitializeComponent();
            LoadUserData();
        }

        public void LoadUserData()
        {
            User user = Authorization.currentUser;

            if (user != null)
            {
                // Загрузка основных данных пользователя
                NameField.Text = user.FirstName;
                LastNameField.Text = user.LastName;
                PhoneField.Text = user.Phone;
                EmailField.Text = user.Email;
                AddressField.Text = user.Address;
                DOBField.SelectedDate = user.DateOfBirth;

                // Загрузка аватара
                LoadAvatar(user.ImagePath);
            }
            else
            {
                CustomMessageBox.Show("Не удалось загрузить данные пользователя.");
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

                // Обрабатываем три варианта:
                // 1. Абсолютный путь (C:\path\to\avatar.png)
                // 2. Относительный путь (../../avatar/boar.png)
                // 3. URI ресурса (pack://application:,,,...)

                if (Path.IsPathRooted(imagePath))
                {
                    // Вариант 1: Абсолютный путь
                    if (File.Exists(imagePath))
                    {
                        SetAvatarImage(imagePath);
                        return;
                    }
                }
                else if (imagePath.StartsWith("pack:"))
                {
                    // Вариант 3: URI ресурса
                    try
                    {
                        AvatarImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                        return;
                    }
                    catch { /* Продолжаем попытки */ }
                }

                // Вариант 2: Относительный путь
                string[] possibleBasePaths = {
            AppDomain.CurrentDomain.BaseDirectory,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", ".."), // Для bin/Debug
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..") // Для bin
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
                    catch { /* Продолжаем проверку других путей */ }
                }

                // Если ни один вариант не сработал
                SetDefaultAvatar();
                MessageBox.Show($"Не удалось загрузить аватар по пути: {imagePath}");
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
            // Попробуем найти стандартный аватар в разных местах
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
                catch { /* Продолжаем попытки */ }
            }

            AvatarImage.Source = null;
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            string firstName = NameField.Text;
            string lastName = LastNameField.Text;
            string phone = PhoneField.Text;
            string email = EmailField.Text;
            string address = AddressField.Text;
            DateTime? dateOfBirth = DOBField.SelectedDate;

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(address) || !dateOfBirth.HasValue)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            User updatedUser = new User
            {
                UserId = Authorization.currentUser.UserId,
                Login = Authorization.currentUser.Login,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                Email = email,
                Address = address,
                DateOfBirth = dateOfBirth.Value,
                ImagePath = Authorization.currentUser.ImagePath
            };

            if (userFromDb.UpdateEmployeeProfile(updatedUser))
            {
                Authorization.currentUser = updatedUser;
                Authorization.NotifyUserUpdated(updatedUser);

                UserFromDb userFromDb = new UserFromDb();
                userFromDb.AddOrUpdateImagePath(updatedUser.UserId, Authorization.currentUser.ImagePath);

                LoadUserData();

                MessageBox.Show("Профиль успешно обновлен.");
            }
            else
            {
                MessageBox.Show("Ошибка при сохранении изменений.");
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
                var avatarsPage = new AvatarsPage(this);
                AvatarFrame.Navigate(avatarsPage);
            }
        }


        public void UpdateAvatar(string newAvatarPath)
        {
            Authorization.currentUser.ImagePath = newAvatarPath;
            AvatarImage.Source = new BitmapImage(new Uri(newAvatarPath));
        }

        private void ToggleCalendarPopup(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.TemplatedParent is DatePicker datePicker)
            {
                var popup = datePicker.Template.FindName("PART_Popup", datePicker) as Popup;
                if (popup != null)
                {
                    popup.IsOpen = !popup.IsOpen;
                }
            }
        }
    }
}
