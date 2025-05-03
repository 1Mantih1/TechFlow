using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TechFlow.Classes;
using TechFlow.Models;

namespace TechFlow.Windows
{
    public partial class CreateProjectOrderWindow : Window
    {
        private int currentStep = 1;
        ProjectFromDb projectFromDb = new ProjectFromDb();
        public CreateProjectOrderWindow()
        {
            InitializeComponent();
            InitializeProjectTypes();
            SetDefaultDates();
            UpdateStepNavigation();
        }

        private void SetDefaultDates()
        {
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today.AddDays(1);
        }

        private void InitializeProjectTypes()
        {
            ProjectTypeComboBox.Items.Add("Веб-разработка");
            ProjectTypeComboBox.Items.Add("Мобильное приложение");
            ProjectTypeComboBox.Items.Add("Дизайн");
            ProjectTypeComboBox.Items.Add("Маркетинг");
            ProjectTypeComboBox.Items.Add("Консалтинг");
            ProjectTypeComboBox.Items.Add("Разработка ПО");
            ProjectTypeComboBox.Items.Add("Тестирование");
            ProjectTypeComboBox.SelectedIndex = 0;
        }

        private void UpdateStepNavigation()
        {
            // Update panels visibility
            Step1Panel.Visibility = currentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
            Step2Panel.Visibility = currentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
            Step3Panel.Visibility = currentStep == 3 ? Visibility.Visible : Visibility.Collapsed;

            // Update navigation buttons
            BackButton.Visibility = currentStep > 1 ? Visibility.Visible : Visibility.Collapsed;
            NextButton.Content = currentStep < 3 ? "ДАЛЕЕ" : "ПОДТВЕРДИТЬ ЗАКАЗ";

            // Update step buttons style
            Step1Button.Style = currentStep == 1 ?
                (Style)FindResource("PrimaryButtonStyle") :
                (Style)FindResource("SecondaryButtonStyle");

            Step2Button.Style = currentStep == 2 ?
                (Style)FindResource("PrimaryButtonStyle") :
                (Style)FindResource("SecondaryButtonStyle");

            Step3Button.Style = currentStep == 3 ?
                (Style)FindResource("PrimaryButtonStyle") :
                (Style)FindResource("SecondaryButtonStyle");

            if (currentStep == 3)
            {
                UpdateConfirmationData();
            }
        }

        private void UpdateConfirmationData()
        {
            ConfirmNameText.Text = ProjectNameTextBox.Text;
            ConfirmDescriptionText.Text = ProjectDescriptionTextBox.Text;
            ConfirmStartDateText.Text = StartDatePicker.SelectedDate?.ToString("dd.MM.yyyy") ?? "Не указана";
            ConfirmEndDateText.Text = EndDatePicker.SelectedDate?.ToString("dd.MM.yyyy") ?? "Не указана";
            ConfirmTypeText.Text = ProjectTypeComboBox.SelectedItem?.ToString() ?? "Не указан";
            ConfirmBudgetText.Text = string.IsNullOrEmpty(BudgetTextBox.Text) ? "Не указан" : $"{BudgetTextBox.Text} руб.";

            var options = new System.Text.StringBuilder();
            if (UrgentCheckBox.IsChecked == true) options.AppendLine("• Срочный проект");
            if (ConfidentialCheckBox.IsChecked == true) options.AppendLine("• Конфиденциальный проект");
            if (string.IsNullOrWhiteSpace(RequirementsTextBox.Text))
                options.AppendLine("• Без дополнительных требований");
            else
                options.AppendLine($"• Требования: {RequirementsTextBox.Text}");

            ConfirmOptionsText.Text = options.ToString();
        }

        private bool ValidateCurrentStep()
        {
            if (currentStep == 1)
            {
                if (string.IsNullOrWhiteSpace(ProjectNameTextBox.Text))
                {
                    ShowValidationError("Введите название проекта");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(ProjectDescriptionTextBox.Text))
                {
                    ShowValidationError("Введите описание проекта");
                    return false;
                }

                if (StartDatePicker.SelectedDate == null)
                {
                    ShowValidationError("Укажите дату начала проекта");
                    return false;
                }

                if (StartDatePicker.SelectedDate < DateTime.Today)
                {
                    ShowValidationError("Дата начала не может быть в прошлом");
                    return false;
                }

                if (EndDatePicker.SelectedDate != null && EndDatePicker.SelectedDate < StartDatePicker.SelectedDate)
                {
                    ShowValidationError("Дата завершения не может быть раньше даты начала");
                    return false;
                }
            }
            else if (currentStep == 2)
            {
                if (ProjectTypeComboBox.SelectedItem == null)
                {
                    ShowValidationError("Выберите тип проекта");
                    return false;
                }
            }

            return true;
        }

        private void ShowValidationError(string message)
        {
            CustomMessageBox.Show(message, "Ошибка валидации");
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateCurrentStep()) return;

            if (currentStep < 3)
            {
                currentStep++;
                UpdateStepNavigation();
            }
            else
            {
                CreateProjectOrder();
            }
        }

        private void CreateProjectOrder()
        {
            try
            {
                int currentClientId = Authorization.currentClient.ClientId;

                bool success = projectFromDb.CreateProject(
                    projectName: ProjectNameTextBox.Text,
                    projectDescription: ProjectDescriptionTextBox.Text,
                    startDate: StartDatePicker.SelectedDate ?? DateTime.Today,
                    endDate: EndDatePicker.SelectedDate,
                    clientId: currentClientId,
                    projectType: ProjectTypeComboBox.SelectedItem?.ToString(),
                    budget: decimal.TryParse(BudgetTextBox.Text, out var budget) ? budget : (decimal?)null,
                    requirements: RequirementsTextBox.Text,
                    isUrgent: UrgentCheckBox.IsChecked == true,
                    isConfidential: ConfidentialCheckBox.IsChecked == true
                );

                if (success)
                {
                    CustomMessageBox.Show("Заказ проекта успешно создан и отправлен на модерацию!", "Успех");
                    this.Close();
                    Authorization authorization = new Authorization();
                    authorization.Show();
                }
                else
                {
                    CustomMessageBox.Show("Не удалось создать проект", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentStep > 1)
            {
                currentStep--;
                UpdateStepNavigation();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomMessageBox.Show("Вы уверены, что хотите отменить создание заказа? Все введенные данные будут потеряны", "Подтверждение") == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                ((PackIcon)((Button)sender).Content).Kind = PackIconKind.WindowMaximize;
            }
            else
            {
                WindowState = WindowState.Maximized;
                ((PackIcon)((Button)sender).Content).Kind = PackIconKind.WindowRestore;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WindowHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateWindowCorners();
        }

        private void UpdateWindowCorners()
        {
            double radius = (WindowState == WindowState.Maximized) ? 0 : 16;

            MainBorder.CornerRadius = new CornerRadius(radius);

            this.Clip = new RectangleGeometry(
                new Rect(0, 0, ActualWidth, ActualHeight),
                radius,
                radius
            );
        }
    }
}