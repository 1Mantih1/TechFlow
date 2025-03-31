using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;

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
                NameField.Text = user.FirstName;
                LastNameField.Text = user.LastName;
                PhoneField.Text = user.Phone;
                EmailField.Text = user.Email;
                AddressField.Text = user.Address;
                DOBField.SelectedDate = user.DateOfBirth;
                AvatarImage.Source = new BitmapImage(new Uri(user.ImagePath));
            }
            else
            {
                CustomMessageBox.Show("Не удалось загрузить данные пользователя.");
            }
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
