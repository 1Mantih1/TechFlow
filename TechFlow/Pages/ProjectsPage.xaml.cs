using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProjectsPage.xaml
    /// </summary>
    public partial class ProjectsPage : Page
    {
        public ICommand AddProjectCommand { get; }
        public ICommand FilterCommand { get; }
        public string SearchText { get; set; }
        public ObservableCollection<Project> Projects { get; set; }
        public event Action<Project> OnProjectSelected;

        public ProjectsPage()
        {
            InitializeComponent();
            LoadProjects();
            DataContext = this;
        }

        private void LoadProjects()
        {
            ProjectFromDb projectFromDb = new ProjectFromDb();

            List<Project> projectList = projectFromDb.LoadProjects();
            Projects = new ObservableCollection<Project>(projectList);
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedProject = (Project)((DataGrid)sender).SelectedItem;
            OnProjectSelected?.Invoke(selectedProject);
        }

        public int CurrentProjectCount
        {
            get
            {
                return Projects.Count;
            }
        }

        public int CurrentActiveProjectsCount
        {
            get
            {
                int activeCount = 0;
                foreach (var project in Projects)
                {
                    if (project.Status == "Активный")
                    {
                        activeCount++;
                    }
                }
                return activeCount;
            }
        }
    }
}
