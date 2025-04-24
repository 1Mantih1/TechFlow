using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    public partial class AttachmentsPage : Page
    {
        private int TaskId { get; set; }
        private FileFromDb fileDb = new FileFromDb();
        private Border _lastHoveredBorder;

        public AttachmentsPage(int taskId)
        {
            InitializeComponent();
            TaskId = taskId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAttachments();
        }

        private void LoadAttachments()
        {
            var files = fileDb.LoadFiles(TaskId)
                .OrderByDescending(f => f.CreationDate)
                .ToList();

            var attachments = files.Select(file => new
            {
                file.FileName,
                file.FilePath,
                file.FileSizeFormatted,
                file.CreationDate,
                FirstName = Authorization.currentUser.FirstName,
                LastName = Authorization.currentUser.LastName
            }).ToList();

            AttachmentsList.ItemsSource = attachments;
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                // Скрываем кнопку у предыдущего элемента
                if (_lastHoveredBorder != null && _lastHoveredBorder != border)
                {
                    HideDownloadButton(_lastHoveredBorder);
                }

                // Показываем кнопку у текущего элемента
                ShowDownloadButton(border);
                _lastHoveredBorder = border;
            }
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            // Не скрываем кнопку сразу при уходе мыши, чтобы можно было нажать на кнопку
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext != null)
            {
                dynamic file = border.DataContext;
                fileDb.DownloadFile(file.FilePath, file.FileName);
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext != null)
            {
                dynamic file = button.DataContext;
                fileDb.DownloadFile(file.FilePath, file.FileName);
                e.Handled = true; // Предотвращаем всплытие события к Border
            }
        }

        private void ShowDownloadButton(Border border)
        {
            var button = FindChild<Button>(border, "DownloadButton");
            if (button != null)
            {
                button.Visibility = Visibility.Visible;
            }
        }

        private void HideDownloadButton(Border border)
        {
            var button = FindChild<Button>(border, "DownloadButton");
            if (button != null)
            {
                button.Visibility = Visibility.Collapsed;
            }
        }

        private T FindChild<T>(DependencyObject parent, string childName = null) where T : DependencyObject
        {
            if (parent == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T result && (childName == null || (child is FrameworkElement fe && fe.Name == childName)))
                {
                    return result;
                }

                var descendant = FindChild<T>(child, childName);
                if (descendant != null)
                {
                    return descendant;
                }
            }
            return null;
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