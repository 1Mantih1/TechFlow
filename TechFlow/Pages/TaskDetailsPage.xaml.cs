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
using TechFlow.Classes;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    /// <summary>
    /// Логика взаимодействия для TaskDetailsPage.xaml
    /// </summary>
    public partial class TaskDetailsPage : Page
    {
        public TaskDetailsPage()
        {
            InitializeComponent();
        }

        private void ButtonChat_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProjectTask task)
            {
                TaskChatPage taskChatPage = new TaskChatPage(task.TaskId);
                NavigationService?.Navigate(taskChatPage);
            }
        }

        private async void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as ProjectManagement;
            if (mainWindow != null)
            {
                await mainWindow.GoBack();
            }
        }

        private void ViewAllAttachments_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProjectTask task)
            {
                NavigationService.Navigate(new AttachmentsPage(task.TaskId));
            }
        }
    }
}
