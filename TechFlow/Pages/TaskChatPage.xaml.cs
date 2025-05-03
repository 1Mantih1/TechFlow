using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Npgsql;
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
        private int _discussionId = -1;

        public TaskChatPage(int taskId)
        {
            InitializeComponent();
            TaskId = taskId;
            UpdateParticipantsCount();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _discussionId = discussionDb.GetDiscussionIdByTaskId(TaskId);

            if (_discussionId == -1)
            {
                CustomMessageBox.Show("Обсуждение для этой задачи не найдено");
                return;
            }

            LoadChatItems();

        }

        private void UpdateParticipantsCount()
        {
            int participantsCount = taskDb.CountTaskParticipants(TaskId);
            ParticipantsTextBlock.Text = $"{participantsCount} участников";
        }

        private void LoadChatItems()
        {
            if (_discussionId == -1) return;

            var chatItems = new List<ChatItem>();
 
            var comments = discussionDb.LoadComments(_discussionId);
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

            chatItems = chatItems.OrderBy(item => item.CreationDate).ToList();

            ChatItemsList.ItemsSource = chatItems;
            UpdateLastMessageTime();
            UpdateParticipantsCount(); 
        }

        private void AddCommentButton_Click(object sender, RoutedEventArgs e)
        {
            if (_discussionId <= 0)
            {
                // Попробуем создать обсуждение, если его нет
                _discussionId = CreateDiscussionForTask();
                if (_discussionId <= 0)
                {
                    CustomMessageBox.Show("Не удалось создать обсуждение для задачи");
                    return;
                }
            }

            string commentText = CommentTextBox.Text.Trim();
            if (string.IsNullOrEmpty(commentText))
            {
                CustomMessageBox.Show("Введите текст комментария");
                return;
            }

            if (discussionDb.AddComment(commentText, _discussionId, Authorization.currentUser.UserId))
            {
                CommentTextBox.Text = string.Empty;
                LoadChatItems();
            }
        }

        private int CreateDiscussionForTask()
        {
            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Получаем название задачи для имени обсуждения
                        var taskTitle = GetTaskTitle(TaskId, connection, transaction);

                        var cmd = new NpgsqlCommand(
                            @"INSERT INTO discussion 
                    (discussion_name, creation_date, task_id) 
                    VALUES (@name, CURRENT_TIMESTAMP, @task_id)
                    RETURNING discussion_id;",
                            connection, transaction);

                        cmd.Parameters.AddWithValue("@name", $"Обсуждение задачи: {taskTitle}");
                        cmd.Parameters.AddWithValue("@task_id", TaskId);

                        var newDiscussionId = cmd.ExecuteScalar();
                        if (newDiscussionId == null)
                        {
                            transaction.Rollback();
                            return -1;
                        }

                        transaction.Commit();
                        return Convert.ToInt32(newDiscussionId);
                    }
                    catch
                    {
                        transaction.Rollback();
                        return -1;
                    }
                }
            }
        }

        private string GetTaskTitle(int taskId, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            var cmd = new NpgsqlCommand(
                "SELECT task_name FROM task WHERE task_id = @task_id",
                connection, transaction);
            cmd.Parameters.AddWithValue("@task_id", taskId);
            return cmd.ExecuteScalar()?.ToString() ?? "Без названия";
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
                    CustomMessageBox.Show("Файл успешно прикреплен!");
                    LoadChatItems();
                    UpdateLastMessageTime();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Ошибка при прикреплении файла: {ex.Message}");
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