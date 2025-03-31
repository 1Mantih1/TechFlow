using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using TechFlow.Classes;
using TechFlow.Models;

namespace TechFlow.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProjectStagesPage.xaml
    /// </summary>
    public partial class ProjectStagesPage : Page
    {
        public ObservableCollection<ProjectStage> ProjectStages { get; set; }
        public event Action<ProjectStage> OnProjectStageSelected;
        public ProjectStagesPage()
        {
            InitializeComponent();
            LoadProjectStages();
            DataContext = this;
        }

        private void LoadProjectStages()
        {
            ProjectStageFromDb projectStageFromDb = new ProjectStageFromDb();
            ProjectStages = new ObservableCollection<ProjectStage>(projectStageFromDb.LoadProjectStages());
        }

        public int CurrentProjectCount
        {
            get
            {
                return ProjectStages.Count;
            }
        }
        public int CurrentActiveProjectsCount
        {
            get
            {
                int activeCount = 0;
                foreach (var stage in ProjectStages)
                {
                    if (stage.Status == "Активный")
                    {
                        activeCount++;
                    }
                }
                return activeCount;
            }
        }


        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedProjectStage = (ProjectStage)((DataGrid)sender).SelectedItem;
            OnProjectStageSelected?.Invoke(selectedProjectStage);
        }
    }
}
