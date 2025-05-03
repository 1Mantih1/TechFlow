using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TechFlow.Classes;
using TechFlow.Windows;
using TechFlow.Models;

namespace TechFlow.Pages
{
    public partial class TimesheetPage : Page
    {
        private readonly TimesheetFromDb _timesheetDb;
        private List<Timesheet> _allTimesheets = new List<Timesheet>();

        public ObservableCollection<TimesheetDisplay> Timesheets { get; } = new ObservableCollection<TimesheetDisplay>();

        public TimesheetPage()
        {
            InitializeComponent();
            _timesheetDb = new TimesheetFromDb();
            DataContext = this;
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadTimesheets();
                CheckAdminVisibility();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при загрузке страницы: {ex.Message}");
            }
        }

        private void CheckAdminVisibility()
        {
            try
            {
                UserFromDb db = new UserFromDb();   
                AdminButton.Visibility = db.IsAdmin(Authorization.currentUser.UserId)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка проверки прав администратора: {ex.Message}");
            }
        }

        private void LoadTimesheets()
        {
            try
            {
                _allTimesheets = _timesheetDb.LoadTimesheets()
                    ?.Where(t => t != null &&
                                Authorization.currentUser != null &&
                                t.EmployeeId == Authorization.currentUser.UserId)
                    ?.ToList() ?? new List<Timesheet>();

                Timesheets.Clear();

                for (int day = 1; day <= 31; day++)
                {
                    var dayEntry = new TimesheetDisplay { Day = day.ToString("00") };

                    for (int month = 1; month <= 12; month++)
                    {
                        var record = _allTimesheets.FirstOrDefault(t =>
                            t.WorkDate.Day == day &&
                            t.WorkDate.Month == month);

                        string status = record?.Status?.Description ?? "";
                        dayEntry.SetWorkType(month - 1, status);
                    }

                    Timesheets.Add(dayEntry);
                }

                UpdateCurrentStatus();
                UpdateUserInfo();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при загрузке табеля: {ex.Message}");
            }
        }

        private void UpdateCurrentStatus()
        {
            try
            {
                var todayRecord = _allTimesheets.FirstOrDefault(t => t.WorkDate.Date == DateTime.Today);
                StatusTextBlock.Text = todayRecord?.Status?.Description ?? "Не определен";

                // Обновляем отображение рабочего времени
                if (todayRecord != null &&
                    todayRecord.Status?.Description == "На рабочем месте" &&
                    todayRecord.StartTime != TimeSpan.Zero &&
                    todayRecord.EndTime != TimeSpan.Zero)
                {
                    TimeWorkedTextBlock.Text = $"{todayRecord.StartTime:hh\\:mm} - {todayRecord.EndTime:hh\\:mm}";
                    TimeWorkedTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    TimeWorkedTextBlock.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка обновления статуса: {ex.Message}");
            }
        }

        private void UpdateUserInfo()
        {
            try
            {
                if (Authorization.currentUser != null)
                {
                    UserTimesheetTextBlock.Text =
                        $"Табель учета времени: {Authorization.currentUser.LastName} " +
                        $"{Authorization.currentUser.FirstName}";
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка обновления информации пользователя: {ex.Message}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTimesheets();
        }

        private void MarkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;
                TimeSpan startWindow = new TimeSpan(8, 0, 0); // 8:00
                TimeSpan endWindow = new TimeSpan(12, 0, 0);  // 12:00

                // Проверяем, что текущее время между 8:00 и 12:00 включительно
                if (currentTime < startWindow || currentTime > endWindow)
                {
                    CustomMessageBox.Show("Отметка возможна только с 8:00 до 12:00 включительно");
                    return;
                }

                var todayRecord = _allTimesheets.FirstOrDefault(t =>
                    t.WorkDate.Date == now.Date);

                if (todayRecord != null && todayRecord.Status?.Description == "На рабочем месте")
                {
                    CustomMessageBox.Show("Вы уже отметились сегодня.");
                    return;
                }

                // Рассчитываем время окончания (начальное время + 8 часов)
                TimeSpan startTime = currentTime;
                TimeSpan endTime = startTime.Add(TimeSpan.FromHours(8));

                // Проверяем, чтобы время окончания не было на следующий день
                if (endTime.Days > 0)
                {
                    endTime = new TimeSpan(endTime.Hours, endTime.Minutes, endTime.Seconds);
                }

                // Сохраняем в базу
                _timesheetDb.UpdateStatus("На рабочем месте", startTime, endTime);

                StatusTextBlock.Text = "На рабочем месте";
                CustomMessageBox.Show($"Вы успешно отметились!\nРабочее время сегодня: {startTime:hh\\:mm} - {endTime:hh\\:mm}");
                LoadTimesheets();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при отметке: {ex.Message}");
            }
        }
        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminTimesheetPage());
        }
    }
}