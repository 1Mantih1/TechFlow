using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    public partial class ProjectStagesPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<ProjectStage> OnProjectStageSelected;

        private string searchText;
        public string SearchText
        {
            get => searchText;
            set
            {
                if (searchText != value)
                {
                    searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    SearchProjectStages();
                }
            }
        }

        private string projectSearchText;
        public string ProjectSearchText
        {
            get => projectSearchText;
            set
            {
                if (projectSearchText != value)
                {
                    projectSearchText = value;
                    OnPropertyChanged(nameof(ProjectSearchText));
                }
            }
        }

        private string selectedStatus = "Все";
        public string SelectedStatus
        {
            get => selectedStatus;
            set
            {
                if (selectedStatus != value)
                {
                    selectedStatus = value;
                    OnPropertyChanged(nameof(SelectedStatus));
                }
            }
        }

        private string selectedDateFilter = "Любая дата";
        public string SelectedDateFilter
        {
            get => selectedDateFilter;
            set
            {
                if (selectedDateFilter != value)
                {
                    selectedDateFilter = value;
                    OnPropertyChanged(nameof(SelectedDateFilter));
                }
            }
        }

        private bool isUrgentFilter;
        public bool IsUrgentFilter
        {
            get => isUrgentFilter;
            set
            {
                if (isUrgentFilter != value)
                {
                    isUrgentFilter = value;
                    OnPropertyChanged(nameof(IsUrgentFilter));
                }
            }
        }

        private ObservableCollection<ProjectStage> projectStages;
        public ObservableCollection<ProjectStage> ProjectStages
        {
            get => projectStages;
            set
            {
                projectStages = value;
                OnPropertyChanged(nameof(ProjectStages));
                OnPropertyChanged(nameof(CurrentProjectCount));
                OnPropertyChanged(nameof(CurrentActiveProjectsCount));
            }
        }

        private readonly ProjectStageFromDb projectStageFromDb = new ProjectStageFromDb();

        public ProjectStagesPage()
        {
            InitializeComponent();
            DataContext = this;
            LoadProjectStages();
        }

        private void LoadProjectStages()
        {
            try
            {
                var stagesList = projectStageFromDb.LoadProjectStages();
                ProjectStages = new ObservableCollection<ProjectStage>(stagesList);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки этапов: {ex.Message}");
            }
        }
        private void ProjectStages_Loaded(object sender, RoutedEventArgs e)
        {
            UserFromDb userFromDb = new UserFromDb();
            int currentEmployeeId = Windows.Authorization.currentUser.UserId;

            if (userFromDb.IsAdmin(currentEmployeeId))
            {
                AddStageButton.Visibility = Visibility.Visible;
            }
            else
            {
                AddStageButton.Visibility = Visibility.Collapsed;
            }
        }


        private void SearchProjectStages()
        {
            try
            {
                var stagesList = projectStageFromDb.FilterProjectStages(
                    searchText: SearchText,
                    projectName: ProjectSearchText,
                    status: SelectedStatus == "Все" ? null : SelectedStatus,
                    dateFilterOption: SelectedDateFilter,
                    isUrgent: IsUrgentFilter
                );

                ProjectStages = new ObservableCollection<ProjectStage>(stagesList);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка поиска: {ex.Message}");
            }
        }


        public int CurrentProjectCount => ProjectStages.Count;

        public int CurrentActiveProjectsCount =>
            ProjectStages.Count(s => s.Status == "Активный");

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((DataGrid)sender).SelectedItem is ProjectStage selectedStage)
            {
                OnProjectStageSelected?.Invoke(selectedStage);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = false;
            SearchProjectStages();
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            SearchText = string.Empty;
            ProjectSearchText = string.Empty;
            SelectedStatus = "Все";
            SelectedDateFilter = "Любая дата";
            IsUrgentFilter = false;

            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(ProjectSearchText));
            OnPropertyChanged(nameof(SelectedStatus));
            OnPropertyChanged(nameof(SelectedDateFilter));
            OnPropertyChanged(nameof(IsUrgentFilter));

            LoadProjectStages();
            FilterPopup.IsOpen = false;
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilterPopup.IsOpen)
            {
                FilterPopup.IsOpen = false;
            }
            else
            {
                FilterPopup.PlacementTarget = sender as Button;
                FilterPopup.IsOpen = true;
            }
        }

        private void AddStageButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectAddStagePage projectAddStagePage = new ProjectAddStagePage();
            NavigationService.Navigate(projectAddStagePage);
        }
    }
}