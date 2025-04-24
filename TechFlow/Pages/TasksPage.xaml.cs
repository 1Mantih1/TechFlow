using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TechFlow.Classes;
using TechFlow.Models;

namespace TechFlow.Pages
{
    public partial class TasksPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<ProjectTask> OnTaskSelected;

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
                    SearchTasks();
                }
            }
        }

        private string taskSearchText;
        public string TaskSearchText
        {
            get => taskSearchText;
            set
            {
                if (taskSearchText != value)
                {
                    taskSearchText = value;
                    OnPropertyChanged(nameof(TaskSearchText));
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

        private string selectedStage = "Все этапы";
        public string SelectedStage
        {
            get => selectedStage;
            set
            {
                if (selectedStage != value)
                {
                    selectedStage = value;
                    OnPropertyChanged(nameof(SelectedStage));
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

        private string stageSearchText;
        public string StageSearchText
        {
            get => stageSearchText;
            set
            {
                if (stageSearchText != value)
                {
                    stageSearchText = value;
                    OnPropertyChanged(nameof(StageSearchText));
                }
            }
        }

        private ObservableCollection<ProjectTask> tasks;
        public ObservableCollection<ProjectTask> Tasks
        {
            get => tasks;
            set
            {
                tasks = value;
                OnPropertyChanged(nameof(Tasks));
                OnPropertyChanged(nameof(CurrentTaskCount));
            }
        }

        private readonly TaskFromDb taskFromDb = new TaskFromDb();

        public TasksPage()
        {
            InitializeComponent();
            DataContext = this;
            LoadTasks();
        }

        private void Task_Loaded(object sender, RoutedEventArgs e)
        {
            UserFromDb userFromDb = new UserFromDb();
            int currentEmployeeId = Windows.Authorization.currentUser.UserId;

            if (userFromDb.IsAdmin(currentEmployeeId))
            {
                AddTaskButton.Visibility = Visibility.Visible;
            }
            else
            {
                AddTaskButton.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadTasks()
        {
            try
            {
                var tasksList = taskFromDb.LoadTasks();
                Tasks = new ObservableCollection<ProjectTask>(tasksList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задач: {ex.Message}");
            }
        }

        private void SearchTasks()
        {
            try
            {
                var tasksList = taskFromDb.FilterTasks(
                    searchText: SearchText,
                    taskSearchText: TaskSearchText,
                    stageSearchText: StageSearchText,
                    status: SelectedStatus == "Все" ? null : SelectedStatus,
                    stage: SelectedStage == "Все этапы" ? null : SelectedStage,
                    dateFilterOption: SelectedDateFilter,
                    isUrgent: IsUrgentFilter
                );

                Tasks = new ObservableCollection<ProjectTask>(tasksList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}");
            }
        }


        public int CurrentTaskCount => Tasks.Count;

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((DataGrid)sender).SelectedItem is ProjectTask selectedTask)
            {
                OnTaskSelected?.Invoke(selectedTask);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = false;
            SearchTasks();
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            SearchText = string.Empty;
            StageSearchText = string.Empty;
            SelectedStatus = "Все";
            SelectedStage = "Все этапы";
            SelectedDateFilter = "Любая дата";
            IsUrgentFilter = false;

            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(StageSearchText));
            OnPropertyChanged(nameof(SelectedStatus));
            OnPropertyChanged(nameof(SelectedStage));
            OnPropertyChanged(nameof(SelectedDateFilter));
            OnPropertyChanged(nameof(IsUrgentFilter));

            LoadTasks();
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

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            AddTaskPage addTaskPage = new AddTaskPage();
            NavigationService.Navigate(addTaskPage);
        }
    }
}