using System.Windows;
using TechFlow.Pages;
using TechFlow.Models;
using TechFlow.Classes;
using System;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace TechFlow.Windows
{
    public partial class ProjectManagement : Window
    {
        ProjectsPage projectsPage = new ProjectsPage();
        ProjectStagesPage projectStagesPage = new ProjectStagesPage();
        TeamsPage teamsPage = new TeamsPage();
        TasksPage tasksPage = new TasksPage();

        public ProjectManagement()
        {
            InitializeComponent();
            Authorization.OnUserUpdated += UpdateUserInfo;
            ContentFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            this.DataContext = projectsPage;
        }

        private void ProjectManagement_Load(object sender, RoutedEventArgs e)
        {
            if (Authorization.currentUser == null)
            {
                MessageBox.Show("Пользователь не авторизован!");
                return;
            }

            string imagePath = Authorization.currentUser.ImagePath;

            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                imagePath = "C:\\Users\\Grigo\\Documents\\TechFlow\\TechFlow\\avatar\\man1.png";
                Authorization.currentUser.ImagePath = imagePath;
            }

            userName.Text = Authorization.currentUser.LastName + " " + Authorization.currentUser.FirstName;

            AvatarImage.Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));

            var projectsPage = new ProjectsPage();
            projectsPage.OnProjectSelected += HandleSelection;
            ContentFrame.Navigate(projectsPage);
        }


        private void UpdateUserInfo(User updatedUser)
        {
            userName.Text = updatedUser.LastName + " " + updatedUser.FirstName;

            if (!string.IsNullOrEmpty(updatedUser.ImagePath))
            {
                AvatarImage.Source = new BitmapImage(new Uri(updatedUser.ImagePath, UriKind.RelativeOrAbsolute));
            }
            else
            {
                AvatarImage.Source = new BitmapImage(new Uri("..\avatar\\man1.png", UriKind.Relative));
            }
        }

        public void HandleSelection(object selectedItem)
        {
            if (selectedItem is Project selectedProject)
            {
                ContentFrame.Navigate(new ProjectDetailsPage { DataContext = selectedProject });
            }
            else if (selectedItem is ProjectStage selectedProjectStage)
            {
                ContentFrame.Navigate(new ProjectStageDetailsPage { DataContext = selectedProjectStage });
            }
            else if (selectedItem is Team selectedTeam)
            {
                ContentFrame.Navigate(new TeamDetailsPage(selectedTeam));
            }
            else if (selectedItem is ProjectTask selectedTask)
            {
                ContentFrame.Navigate(new TaskDetailsPage(selectedTask.TaskId));
            }

        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Authorization authorization = new Authorization();
            authorization.Show();
            this.Close();
        }

        
        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new AdminPanelPage());
        }

        private void TimesheetButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateButton(TimesheetButton);
            ContentFrame.Navigate(new TimesheetPage());
        }

        private void Avatar_Click(object sender, MouseButtonEventArgs e)
        {
            Storyboard clickAnimation = (Storyboard)this.Resources["AvatarClickAnimation"];
            clickAnimation.Stop();

            clickAnimation.Begin();

            ContentFrame.Navigate(new EditProfilePage());

        }


        private void TaskButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateButton(TaskButton);
            tasksPage.OnTaskSelected += HandleSelection;
            ContentFrame.Navigate(tasksPage);
            this.DataContext = tasksPage;
        }


        private void ProjectButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateButton(ProjectButton);
            projectsPage.OnProjectSelected += HandleSelection;
            ContentFrame.Navigate(projectsPage);
            this.DataContext = projectsPage;
        }

        private void TeamButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateButton(TeamButton);
            teamsPage.OnTeamSelected += HandleSelection;
            ContentFrame.Navigate(teamsPage);
            this.DataContext = teamsPage;
        }

        private void ProjectStageButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateButton(ProjectStageButton);
            projectStagesPage.OnProjectStageSelected += HandleSelection;
            ContentFrame.Navigate(projectStagesPage);
            this.DataContext = projectStagesPage;
        }

        private void ActivateButton(Button activeButton)
        {
            TimesheetButton.Style = (Style)FindResource("MenuButtonStyle");
            ProjectButton.Style = (Style)FindResource("MenuButtonStyle");
            ProjectStageButton.Style = (Style)FindResource("MenuButtonStyle");
            TaskButton.Style = (Style)FindResource("MenuButtonStyle");
            TeamButton.Style = (Style)FindResource("MenuButtonStyle");

            activeButton.Style = (Style)FindResource("ActiveMenuButtonStyle");
        }
    }
}
