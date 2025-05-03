using System;
using System.Windows;
using System.Windows.Controls;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;

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
                if (string.IsNullOrWhiteSpace(TeamNameField.Text))
                {
                    CustomMessageBox.Show("Введите название команды", "Ошибка");
                    return;
                }

                if (OrganizationDateField.SelectedDate == null)
                {
                    CustomMessageBox.Show("Укажите дату организации команды", "Ошибка");
                    return;
                }

                var newTeam = new Team
                {
                    TeamName = TeamNameField.Text,
                    TeamDescription = DescriptionField.Text,
                    OrganizationDate = OrganizationDateField.SelectedDate.Value,
                    CompletionDate = CompletionDateField.SelectedDate
                };

                int teamId = _teamFromDb.AddTeam(newTeam);

                if (teamId > 0)
                {
                    CustomMessageBox.Show($"Команда успешно добавлена (ID: {teamId})", "Успех");


                    if (NavigationService.CanGoBack)
                    {
                        NavigationService.GoBack();
                    }
                }
                else
                {
                    CustomMessageBox.Show("Не удалось добавить команду",
                                 "Ошибка");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при добавлении команды: {ex.Message}",
                             "Ошибка");
            }
        }
    }
}