using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
                MessageBox.Show($"Ошибка при загрузке страницы: {ex.Message}");
            }
        }

        private void CheckAdminVisibility()
        {
            try
            {
                AdminButton.Visibility = _timesheetDb.IsAdmin(Authorization.currentUser.UserId)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки прав администратора: {ex.Message}");
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
                MessageBox.Show($"Ошибка при загрузке табеля: {ex.Message}");
            }
        }

        private void UpdateCurrentStatus()
        {
            try
            {
                var todayRecord = _allTimesheets.FirstOrDefault(t => t.WorkDate.Date == DateTime.Today);

                if (todayRecord != null)
                {
                    StatusTextBlock.Text = todayRecord.Status?.Description ?? "Не определен";

                    if (todayRecord.Status?.Description == "На рабочем месте")
                    {
                        var startTime = _timesheetDb.GetCheckInTime(Authorization.currentUser.UserId, DateTime.Today);
                        if (startTime != null)
                        {
                            var endTime = startTime.Value.AddHours(8);
                            TimeWorkedTextBlock.Text = $"{startTime.Value:HH:mm} - {endTime:HH:mm}";
                            TimeWorkedTextBlock.Visibility = Visibility.Visible;
                            return;
                        }
                    }
                }

                StatusTextBlock.Text = "Не определен";
                TimeWorkedTextBlock.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления статуса: {ex.Message}");
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
                MessageBox.Show($"Ошибка обновления информации пользователя: {ex.Message}");
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
                var currentTime = DateTime.Now.TimeOfDay;
                var startWindow = new TimeSpan(8, 0, 0); // 8:00
                var endWindow = new TimeSpan(12, 0, 0); // 12:00

                if (currentTime < startWindow || currentTime > endWindow)
                {
                    MessageBox.Show("Отметка возможна только с 8:00 до 12:00");
                    return;
                }

                var todayRecord = _allTimesheets.FirstOrDefault(t =>
                    t.WorkDate.Date == DateTime.Today);

                if (todayRecord != null && todayRecord.Status?.Description == "На рабочем месте")
                {
                    MessageBox.Show("Вы уже отметились сегодня.");
                    return;
                }

                _timesheetDb.UpdateStatus("На рабочем месте", DateTime.Now);

                // Обновляем UI
                var start = DateTime.Now;
                var end = start.AddHours(8);

                StatusTextBlock.Text = "На рабочем месте";
                TimeWorkedTextBlock.Text = $"{start:HH:mm} - {end:HH:mm}";
                TimeWorkedTextBlock.Visibility = Visibility.Visible;

                MessageBox.Show("Вы успешно отметились!");
                LoadTimesheets();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отметке: {ex.Message}");
            }
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            // Здесь можно открыть окно для управления графиком сотрудников
            //var adminWindow = new AdminTimesheetWindow();
            //adminWindow.ShowDialog();
        }
    }

    public class TimesheetDisplay
    {
        public string Day { get; set; } = "";
        public string[] MonthStatuses { get; } = new string[12];

        public void SetWorkType(int monthIndex, string workType)
        {
            if (monthIndex >= 0 && monthIndex < 12)
            {
                MonthStatuses[monthIndex] = workType ?? "";
            }
        }

        public string January => MonthStatuses[0];
        public string February => MonthStatuses[1];
        public string March => MonthStatuses[2];
        public string April => MonthStatuses[3];
        public string May => MonthStatuses[4];
        public string June => MonthStatuses[5];
        public string July => MonthStatuses[6];
        public string August => MonthStatuses[7];
        public string September => MonthStatuses[8];
        public string October => MonthStatuses[9];
        public string November => MonthStatuses[10];
        public string December => MonthStatuses[11];
    }
}