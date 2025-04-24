using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    /// <summary>
    /// Логика взаимодействия для TeamDetailsPage.xaml
    /// </summary>
    public partial class TeamDetailsPage : Page
    {
        private Team selectedTeam;
        public TeamDetailsPage(Team team)
        {
            InitializeComponent();
            selectedTeam = team;
            DataContext = selectedTeam;
        }

        private async void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as ProjectManagement;
            if (mainWindow != null)
            {
                await mainWindow.GoBack();
            }
        }

        private void ButtonTeamMembers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new TeamEmployeesPage(selectedTeam.TeamId));
        }

    }
}
