using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    public partial class TaskChatPage : Page
    {
        private TaskFromDb taskDb = new TaskFromDb();
        private int TaskId { get; set; }
        private DiscussionFromDb discussionDb = new DiscussionFromDb();
        private FileFromDb fileDb = new FileFromDb();

        public TaskChatPage(int taskId)
        {
            InitializeComponent();
            TaskId = taskId;
            UpdateParticipantsCount();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadChatItems();
        }

        private void UpdateParticipantsCount()
        {
            int participantsCount = taskDb.CountTaskParticipants(TaskId);
            ParticipantsTextBlock.Text = $"{participantsCount} участников";
        }

        private void LoadChatItems()
        {
            var chatItems = new List<ChatItem>();

            // Загружаем комментарии
            var comments = discussionDb.LoadComments(TaskId);
            foreach (var comment in comments)
            {
                chatItems.Add(new ChatItem
                {
                    Content = comment,
                    HorizontalAlignment = (comment.EmployeeId == Authorization.currentUser.UserId)
                        ? HorizontalAlignment.Right
                        : HorizontalAlignment.Left,
                    FirstName = comment.FirstName,
                    LastName = comment.LastName,
                    ImagePath = comment.ImagePath,
                    CreationDate = comment.CreationDate,
                    IsFile = false
                });
            }

            // Загружаем файлы
            var files = fileDb.LoadFiles(TaskId);
            foreach (var file in files)
            {
                chatItems.Add(new ChatItem
                {
                    Content = file,
                    HorizontalAlignment = (file.EmployeeId == Authorization.currentUser.UserId)
                        ? HorizontalAlignment.Right
                        : HorizontalAlignment.Left,
                    FirstName = Authorization.currentUser.FirstName,
                    LastName = Authorization.currentUser.LastName,
                    ImagePath = Authorization.currentUser.ImagePath,
                    CreationDate = file.CreationDate,
                    IsFile = true
                });
            }

            // Сортируем по дате создания (от старых к новым)
            chatItems = chatItems.OrderBy(item => item.CreationDate).ToList();

            ChatItemsList.ItemsSource = chatItems;
            UpdateLastMessageTime();
            UpdateParticipantsCount(); // Добавлен вызов обновления счетчика участников
        }

        private void AddCommentButton_Click(object sender, RoutedEventArgs e)
        {
            string commentText = CommentTextBox.Text;

            if (string.IsNullOrWhiteSpace(commentText))
            {
                CustomMessageBox.Show("Введите текст комментария!");
                return;
            }

            discussionDb.AddComment(commentText, TaskId, Authorization.currentUser.UserId);
            MessageBox.Show("Комментарий добавлен!");

            LoadChatItems();
            CommentTextBox.Text = string.Empty;
            UpdateLastMessageTime();
        }

        private void AddTaskFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Выберите файл для прикрепления"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    fileDb.AddFile(TaskId, Authorization.currentUser.UserId, openFileDialog.FileName);
                    MessageBox.Show("Файл успешно прикреплен!");
                    LoadChatItems();
                    UpdateLastMessageTime();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при прикреплении файла: {ex.Message}");
                }
            }
        }

        private void DownloadFile_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ChatItem item && item.IsFile && item.Content is TaskFile file)
            {
                fileDb.DownloadFile(file.FilePath, file.FileName);
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private DateTime? GetLastMessageTime()
        {
            var lastComment = discussionDb.LoadComments(TaskId)
                .OrderByDescending(c => c.CreationDate)
                .FirstOrDefault();

            var lastFile = fileDb.LoadFiles(TaskId)
                .OrderByDescending(f => f.CreationDate)
                .FirstOrDefault();

            if (lastComment == null && lastFile == null)
                return null;

            if (lastComment == null)
                return lastFile.CreationDate;

            if (lastFile == null)
                return lastComment.CreationDate;

            return lastComment.CreationDate > lastFile.CreationDate
                ? lastComment.CreationDate
                : lastFile.CreationDate;
        }

        private void UpdateLastMessageTime()
        {
            var lastMessageTime = GetLastMessageTime();
            if (lastMessageTime.HasValue)
            {
                LastMessageTextBlock.Text = $"Последнее сообщение: {lastMessageTime.Value.ToString("HH:mm")}";
            }
            else
            {
                LastMessageTextBlock.Text = "Нет сообщений";
            }
        }
    }
}