using System;
using System.Windows;
using System.Windows.Controls;

namespace TechFlow.Windows
{
    public partial class CreateProjectOrderWindow : Window
    {
        private int currentStep = 1;

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
            MessageBox.Show(message, "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                // Здесь будет код сохранения проекта в БД
                MessageBox.Show("Заказ проекта успешно создан! Менеджер свяжется с вами для уточнения деталей.",
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (MessageBox.Show("Вы уверены, что хотите отменить создание заказа? Все введенные данные будут потеряны.",
                              "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }
    }
}