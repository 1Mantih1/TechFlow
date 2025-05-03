using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    public partial class AdminPanelPage : Page
    {
        private List<User> employeesToModerate;
        UserFromDb userFromDb = new UserFromDb();
        TimesheetFromDb timesheetFromDb = new TimesheetFromDb();

        public AdminPanelPage()
        {
            InitializeComponent();
            LoadModerationData();
            LoadUnmoderatedProjects();
        }

        private void LoadModerationData()
        {
            employeesToModerate = userFromDb.LoadEmployeesWithDefaultRole();
            EmployeesList.ItemsSource = employeesToModerate;
        }

        private void LoadUnmoderatedProjects()
        {
            var projectFromDb = new ProjectFromDb();
            UnmoderatedProjectsList.ItemsSource = projectFromDb.LoadModeratingProjects();
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int projectId)
            {
                ProjectDetailsPage detailsPage = new ProjectDetailsPage(projectId);
                NavigationService?.Navigate(detailsPage);
            }
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int projectId)
            {
                var projectFromDb = new ProjectFromDb();
                if (projectFromDb.ApproveProject(projectId))
                {
                    CustomMessageBox.Show("Проект утвержден");
                    LoadUnmoderatedProjects();
                }
            }
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int projectId)
            {
                var projectFromDb = new ProjectFromDb();
                if (projectFromDb.RejectProject(projectId))
                {
                    CustomMessageBox.Show("Проект отклонен");
                    LoadUnmoderatedProjects();
                }
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is User user)
            {
                if (userFromDb.UpdateEmployeeRole(user.UserId, "Сотрудник"))
                {
                    timesheetFromDb.CreateInitialTimesheet(user.UserId);

                    CustomMessageBox.Show("Сотрудник принят и добавлен в табель");
                    LoadModerationData();
                }
            }
        }

        private void RejectUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is User user)
            {
                if (userFromDb.DeleteUser(user.UserId))
                {
                    CustomMessageBox.Show("Сотрудник отклонен и удален из системы");
                    LoadModerationData();
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}