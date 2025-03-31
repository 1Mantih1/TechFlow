using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TechFlow.Classes;
using TechFlow.Models;

namespace TechFlow.Pages
{
    /// <summary>
    /// Логика взаимодействия для TeamsPage.xaml
    /// </summary>
    public partial class TeamsPage : Page
    {
        public ObservableCollection<Team> Teams { get; set; }
        public event Action<Team> OnTeamSelected;
        public TeamsPage()
        {
            InitializeComponent();
            LoadTeams();
            DataContext = this;
        }

        private void LoadTeams()
        {
            TeamFromDb teamFromDb = new TeamFromDb();

            List<Team> teamList = teamFromDb.LoadTeams();
            Teams = new ObservableCollection<Team>(teamList);
        }

        public int CurrentProjectCount
        {
            get
            {
                return Teams.Count;
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedTeam = (Team)((DataGrid)sender).SelectedItem;
            OnTeamSelected?.Invoke(selectedTeam);
        }
    }
}
