using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TechFlow.Models;

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
                    MessageBox.Show("Проект утвержден");
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
                    MessageBox.Show("Проект отклонен");
                    LoadUnmoderatedProjects();
                }
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is User user)
            {
                // Устанавливаем роль "Разработчик" по умолчанию
                if (userFromDb.UpdateEmployeeRole(user.UserId, "Разработчик"))
                {
                    // Создаем запись в табеле
                    timesheetFromDb.CreateInitialTimesheet(user.UserId);

                    MessageBox.Show("Сотрудник принят и добавлен в табель");
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
                    MessageBox.Show("Сотрудник отклонен и удален из системы");
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