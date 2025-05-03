using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TechFlow.Classes;
using TechFlow.Windows;

namespace TechFlow.Pages
{
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
