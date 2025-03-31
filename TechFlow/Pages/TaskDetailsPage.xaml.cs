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
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    /// <summary>
    /// Логика взаимодействия для TaskDetailsPage.xaml
    /// </summary>
    public partial class TaskDetailsPage : Page
    {
        private int TaskId { get; set; }
        //private Frame ParentFrame { get; set; }
        public TaskDetailsPage(int taskId)
        {
            InitializeComponent();
            TaskId = taskId;
        }

        private DiscussionFromDb discussionDb = new DiscussionFromDb();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var comments = discussionDb.LoadComments(TaskId);

            foreach (var comment in comments)
            {
                comment.HorizontalAlignment = (comment.EmployeeId == Authorization.currentUser.UserId)
                    ? HorizontalAlignment.Right
                    : HorizontalAlignment.Left;
            }

            CommentList.ItemsSource = comments;
        }





        private void AddCommentButton_Click(object sender, RoutedEventArgs e)
        {
            string commentText = CommentTextBox.Text;

            if (string.IsNullOrWhiteSpace(commentText))
            {
                CustomMessageBox.Show("Введите текст комментария!");
                return;
            }

            int discussionId = TaskId; 
            int teamEmployeeId = Authorization.currentUser.UserId; 

            discussionDb.AddComment(commentText, discussionId, teamEmployeeId);

            MessageBox.Show("Комментарий добавлен!");

            var comments = discussionDb.LoadComments(discussionId);

            foreach (var comment in comments)
            {
                comment.HorizontalAlignment = (comment.EmployeeId == Authorization.currentUser.UserId)
                    ? HorizontalAlignment.Right
                    : HorizontalAlignment.Left;
            }

            CommentList.ItemsSource = null;  
            CommentList.ItemsSource = comments; 

            CommentTextBox.Text = string.Empty;
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void AddTaskFileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CommentTextBox.Text.Length > 200)
            {
                MessageBox.Show("Комментарий не должен превышать 200 символов.");
                CommentTextBox.Text = CommentTextBox.Text.Substring(0, 200);
            }
        }
    }

}
