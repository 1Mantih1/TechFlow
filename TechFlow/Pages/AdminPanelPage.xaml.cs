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

namespace TechFlow.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdminPanelPage.xaml
    /// </summary>
    public partial class AdminPanelPage : Page
    {
        public AdminPanelPage()
        {
            InitializeComponent();
        }

        private void AdminTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Получаем выбранную вкладку
            var selectedTab = AdminTabs.SelectedItem as TabItem;

            if (selectedTab != null)
            {
                // Проверка, какой TabItem был выбран
                if (selectedTab.Header.ToString() == "Проекты")
                {
                    // Логика для вкладки "Проекты"
                    Console.WriteLine("Вы выбрали вкладку Проекты");
                }
                else if (selectedTab.Header.ToString() == "Модерация")
                {
                    // Логика для вкладки "Модерация"
                    Console.WriteLine("Вы выбрали вкладку Модерация");
                }
            }
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
