using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    public partial class TeamsPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<Team> OnTeamSelected;

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
                    SearchTeams();
                }
            }
        }

        private string teamSearchText;
        public string TeamSearchText
        {
            get => teamSearchText;
            set
            {
                if (teamSearchText != value)
                {
                    teamSearchText = value;
                    OnPropertyChanged(nameof(TeamSearchText));
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

        private bool activeOnlyFilter;
        public bool ActiveOnlyFilter
        {
            get => activeOnlyFilter;
            set
            {
                if (activeOnlyFilter != value)
                {
                    activeOnlyFilter = value;
                    OnPropertyChanged(nameof(ActiveOnlyFilter));
                }
            }
        }

        private ObservableCollection<Team> teams;
        public ObservableCollection<Team> Teams
        {
            get => teams;
            set
            {
                teams = value;
                OnPropertyChanged(nameof(Teams));
                OnPropertyChanged(nameof(CurrentTeamCount));
            }
        }

        private readonly TeamFromDb teamFromDb = new TeamFromDb();

        public TeamsPage()
        {
            InitializeComponent();
            DataContext = this;
            LoadTeams();
        }

        private void Teams_Loaded(object sender, RoutedEventArgs e)
        {
            UserFromDb userFromDb = new UserFromDb();
            int currentEmployeeId = Windows.Authorization.currentUser.UserId;

            if (userFromDb.IsAdmin(currentEmployeeId))
            {
                AddTeamButton.Visibility = Visibility.Visible;
                AddEmployeeButton.Visibility = Visibility.Visible;
            }
            else
            {
                AddTeamButton.Visibility = Visibility.Collapsed;
                AddEmployeeButton.Visibility = Visibility.Collapsed;
            }
        }


        private void LoadTeams()
        {
            try
            {
                Teams = new ObservableCollection<Team>(teamFromDb.LoadTeams());
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки команд: {ex.Message}");
            }
        }

        private void SearchTeams()
        {
            try
            {
                var teamsList = teamFromDb.FilterTeams(
                    searchText: TeamSearchText,
                    taskSearchText: TaskSearchText,
                    dateFilterOption: SelectedDateFilter,
                    activeOnly: ActiveOnlyFilter
                );

                Teams = new ObservableCollection<Team>(teamsList);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка поиска: {ex.Message}");
            }
        }

        public int CurrentTeamCount => Teams.Count;

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((DataGrid)sender).SelectedItem is Team selectedTeam)
            {
                OnTeamSelected?.Invoke(selectedTeam);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = false;
            SearchTeams();
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            TeamSearchText = string.Empty;
            TaskSearchText = string.Empty;
            SelectedDateFilter = "Любая дата";
            ActiveOnlyFilter = false;

            OnPropertyChanged(nameof(TeamSearchText));
            OnPropertyChanged(nameof(TaskSearchText));
            OnPropertyChanged(nameof(SelectedDateFilter));
            OnPropertyChanged(nameof(ActiveOnlyFilter));

            LoadTeams();
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

        private void AddTeamButton_Click(object sender, RoutedEventArgs e)
        {
            AddTeamPage addTeamPage = new AddTeamPage();
            NavigationService.Navigate(addTeamPage);
        }
        private void AddTeamMemberButton_Click(object sender, RoutedEventArgs e)
        {
            AddTeamMemberPage addTeamMemberPage = new AddTeamMemberPage();
            NavigationService.Navigate(addTeamMemberPage);
        }
    }
}
