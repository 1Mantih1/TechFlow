using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    public partial class ProjectsPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<Project> OnProjectSelected;

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

        private string selectedSort = "По умолчанию";
        public string SelectedSort
        {
            get => selectedSort;
            set
            {
                if (selectedSort != value)
                {
                    selectedSort = value;
                    OnPropertyChanged(nameof(SelectedSort));
                }
            }
        }

        private string clientSearchText;
        public string ClientSearchText
        {
            get => clientSearchText;
            set
            {
                if (clientSearchText != value)
                {
                    clientSearchText = value;
                    OnPropertyChanged(nameof(ClientSearchText));
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

        private bool isConfidentialFilter;
        public bool IsConfidentialFilter
        {
            get => isConfidentialFilter;
            set
            {
                if (isConfidentialFilter != value)
                {
                    isConfidentialFilter = value;
                    OnPropertyChanged(nameof(IsConfidentialFilter));
                }
            }
        }

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
                    SearchProjects();
                }
            }
        }

        private ObservableCollection<Project> projects;
        public ObservableCollection<Project> Projects
        {
            get => projects;
            set
            {
                projects = value;
                OnPropertyChanged(nameof(Projects));
                OnPropertyChanged(nameof(CurrentProjectCount));
                OnPropertyChanged(nameof(CurrentActiveProjectsCount));
            }
        }

        private readonly ProjectFromDb projectFromDb = new ProjectFromDb();

        public ProjectsPage()
        {
            InitializeComponent();
            DataContext = this;
            LoadProjects();
        }

        private void LoadProjects()
        {
            try
            {
                var projectList = projectFromDb.LoadProjects();
                Projects = new ObservableCollection<Project>(projectList);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки проектов: {ex.Message}");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SearchProjects()
        {
            try
            {
                var projectsList = projectFromDb.FilterProjects(
                    searchText: SearchText,
                    status: SelectedStatus == "Все" ? null : SelectedStatus,
                    clientName: ClientSearchText,
                    isUrgent: IsUrgentFilter,
                    isConfidential: IsConfidentialFilter
                );

                switch (SelectedSort)
                {
                    case "Близкие к завершению":
                        projectsList = projectsList
                            .Where(p => p.EndDate.HasValue)
                            .OrderBy(p => p.EndDate)
                            .ToList();
                        break;
                    case "Новые проекты":
                        projectsList = projectsList
                            .OrderByDescending(p => p.StartDate)
                            .ToList();
                        break;
                    case "Срочные":
                        projectsList = projectsList
                            .OrderByDescending(p => p.IsUrgent)
                            .ThenBy(p => p.EndDate)
                            .ToList();
                        break;
                    case "По умолчанию":
                    default:
                        break;
                }

                Projects = new ObservableCollection<Project>(projectsList);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка поиска: {ex.Message}");
            }
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = false;
            SearchProjects();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((DataGrid)sender).SelectedItem is Project selectedProject)
            {
                OnProjectSelected?.Invoke(selectedProject);
            }
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            SearchText = string.Empty;
            ClientSearchText = string.Empty;
            SelectedStatus = "Все";
            SelectedSort = "По умолчанию";
            IsUrgentFilter = false;
            IsConfidentialFilter = false;

            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(ClientSearchText));
            OnPropertyChanged(nameof(SelectedStatus));
            OnPropertyChanged(nameof(SelectedSort));
            OnPropertyChanged(nameof(IsUrgentFilter));
            OnPropertyChanged(nameof(IsConfidentialFilter));

            LoadProjects();
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

        public int CurrentProjectCount => Projects.Count;

        public int CurrentActiveProjectsCount =>
            Projects.Count(p => p.Status == "Активный");
    }
}