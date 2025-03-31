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
using System.Windows.Shapes;
using TechFlow.Classes;

namespace TechFlow.Windows
{
    /// <summary>
    /// Логика взаимодействия для ProjectInfo.xaml
    /// </summary>
    public partial class ProjectInfo : Window
    {
        public Project SelectedProject { get; set; }
        public ProjectInfo(Project project)
        {
            InitializeComponent();
            SelectedProject = project;
            LoadProjectDetails();
        }

        private void LoadProjectDetails()
        {
            ProjectNameText.Text = SelectedProject.ProjectName;
            ProjectDescriptionText.Text = SelectedProject.ProjectDescription;
            StartDateText.Text = SelectedProject.StartDate.ToString("d");
            EndDateText.Text = SelectedProject.EndDate.HasValue ? SelectedProject.EndDate.Value.ToString("d") : "N/A";
            ClientNameText.Text = SelectedProject.ClientName;
            StatusText.Text = SelectedProject.Status;
        }
    }
}
