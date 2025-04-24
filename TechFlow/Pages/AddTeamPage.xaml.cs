using System;
using System.Windows;
using System.Windows.Controls;
using TechFlow.Classes;
using TechFlow.Models;

namespace TechFlow.Pages
{
    public partial class AddTeamPage : Page
    {
        private readonly TeamFromDb _teamFromDb = new TeamFromDb();

        public AddTeamPage()
        {
            InitializeComponent();
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void AddTeamButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация данных
                if (string.IsNullOrWhiteSpace(TeamNameField.Text))
                {
                    MessageBox.Show("Введите название команды", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (OrganizationDateField.SelectedDate == null)
                {
                    MessageBox.Show("Укажите дату организации команды", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Создание объекта команды
                var newTeam = new Team
                {
                    TeamName = TeamNameField.Text,
                    TeamDescription = DescriptionField.Text,
                    OrganizationDate = OrganizationDateField.SelectedDate.Value,
                    CompletionDate = CompletionDateField.SelectedDate
                };

                // Добавление команды в базу данных
                int teamId = _teamFromDb.AddTeam(newTeam);

                if (teamId > 0)
                {
                    MessageBox.Show($"Команда успешно добавлена (ID: {teamId})",
                                 "Успех",
                                 MessageBoxButton.OK,
                                 MessageBoxImage.Information);

                    // Возврат на предыдущую страницу
                    if (NavigationService.CanGoBack)
                    {
                        NavigationService.GoBack();
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось добавить команду",
                                 "Ошибка",
                                 MessageBoxButton.OK,
                                 MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении команды: {ex.Message}",
                             "Ошибка",
                             MessageBoxButton.OK,
                             MessageBoxImage.Error);
            }
        }
    }
}