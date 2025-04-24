using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProjectDetailsPage.xaml
    /// </summary>
    public partial class ProjectDetailsPage : Page
    {
        private int projectId;
        public ProjectDetailsPage(int projectId)
        {
            InitializeComponent();
            this.projectId = projectId;
            LoadProjectDetails();
        }

        private void LoadProjectDetails()
        {
            var projectFromDb = new ProjectFromDb();
            var project = projectFromDb.GetProjectById(projectId);

            DataContext = project;
        }

        private async void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as ProjectManagement;
            if (mainWindow != null)
            {
                await mainWindow.GoBack();
            }
        }
    }
}
