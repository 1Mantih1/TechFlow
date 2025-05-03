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
    public partial class TeamEmployeesPage : Page
    {
        public ObservableCollection<TeamEmployee> TeamEmployees { get; set; }
        public int TeamId { get; set; }
        public TeamEmployeesPage(int teamId)
        {
            InitializeComponent();
            TeamId = teamId;
            LoadTeamEmployees();
            DataContext = this;
        }

        private void LoadTeamEmployees()
        {
            TeamEmployeeFromDb teamEmployeeFromDb = new TeamEmployeeFromDb();
            List<TeamEmployee> teamEmployeeList = teamEmployeeFromDb.LoadTeamEmployees(TeamId);

            TeamEmployees = new ObservableCollection<TeamEmployee>(teamEmployeeList);

        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
