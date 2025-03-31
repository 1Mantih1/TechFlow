using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    public partial class TimesheetPage : Page
    {
        public ObservableCollection<TimesheetDisplay> Timesheets { get; set; }

        public TimesheetPage()
        {
            InitializeComponent();
            this.DataContext = this;
            Timesheets = new ObservableCollection<TimesheetDisplay>();
            LoadTimesheets();
        }

        private void LoadTimesheets()
        {
            TimesheetFromDb timesheetDb = new TimesheetFromDb();
            List<Timesheet> timesheetList = timesheetDb.LoadTimesheets()
                                                        .Where(t => t.EmployeeId == Authorization.currentUser.UserId)
                                                        .ToList();

            if (timesheetList.Count == 0)
            {
                CustomMessageBox.Show("Нет данных для отображения!");
                return;
            }

            var currentTimesheet = timesheetList.FirstOrDefault(t => t.WorkDate.Date == DateTime.Now.Date); // Фильтруем по текущей дате
            if (currentTimesheet != null)
            {
                string status = currentTimesheet.Status.Description;
                StatusTextBlock.Text = $"Статус задачи: {status}";

                // Проверка статуса и отображение времени работы
                if (status == "На рабочем месте")
                {
                    DateTime workStartTime = DateTime.Now;
                    DateTime workEndTime = workStartTime.AddHours(8);
                    if (workEndTime.Hour >= 20)
                    {
                        workEndTime = new DateTime(workStartTime.Year, workStartTime.Month, workStartTime.Day, 20, 0, 0);
                    }

                    string timeWorked = $"{workStartTime:HH:mm} - {workEndTime:HH:mm}";
                    TimeWorkedTextBlock.Text = $"Ваше текущее время работы: {timeWorked}";

                    TimeWorkedTextBlock.Visibility = Visibility.Visible; // Показываем блок времени работы
                }
                else
                {
                    TimeWorkedTextBlock.Visibility = Visibility.Collapsed; // Скрываем блок времени работы, если не на рабочем месте
                }
            }
            else
            {
                StatusTextBlock.Text = "Не определен";
                TimeWorkedTextBlock.Visibility = Visibility.Collapsed; // Скрываем блок времени работы, если нет данных на текущий день
            }

            UserTimesheetTextBlock.Text = "Занятость сотрудника" + "\n" +
                                          Authorization.currentUser.LastName + " " +
                                          Authorization.currentUser.FirstName;

            Timesheets.Clear(); // Очистить коллекцию перед добавлением новых данных

            for (int day = 1; day <= 31; day++)
            {
                var dayEntry = new TimesheetDisplay { Day = day.ToString("D2") };

                foreach (var monthIndex in Enumerable.Range(1, 12))
                {
                    var dayRecord = timesheetList.FirstOrDefault(t => t.WorkDate.Month == monthIndex && t.WorkDate.Day == day);
                    string statusDescription = dayRecord?.Status.Description ?? ""; // Берём описание статуса

                    // Присваиваем статус соответствующему месяцу
                    string monthName = new DateTime(1, monthIndex, 1).ToString("MMMM", new System.Globalization.CultureInfo("ru-RU"));
                    dayEntry.SetWorkType(monthName, statusDescription);
                }

                Timesheets.Add(dayEntry);  // Добавляем запись в коллекцию
            }
        }




        private void MarkButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime currentLocalTime = DateTime.Now;

            // Проверка, что сегодня уже нельзя работать (если статус уже "Завершил работу")
            TimesheetFromDb timesheetDb = new TimesheetFromDb();
            List<Timesheet> timesheetList = timesheetDb.LoadTimesheets()
                                                        .Where(t => t.EmployeeId == Authorization.currentUser.UserId &&
                                                                    t.WorkDate.Date == currentLocalTime.Date)  // Проверка по текущей дате
                                                        .ToList();

            var currentTimesheet = timesheetList.FirstOrDefault(t => t.Status.Description == "Завершил работу");
            if (currentTimesheet != null)
            {
                MessageBox.Show("Вы уже завершили работу сегодня.");
                return;  // Если уже завершена работа, не позволяем обновить статус
            }

            // Обновляем статус на "На рабочем месте"
            timesheetDb.UpdateStatus("На рабочем месте");

            // Обновляем статус на UI
            StatusTextBlock.Text = "Статус задачи: На рабочем месте";

            // Показываем TimeWorkedTextBlock, если сотрудник на рабочем месте
            TimeWorkedTextBlock.Visibility = Visibility.Visible;

            DateTime workStartTime = currentLocalTime;
            DateTime workEndTime = currentLocalTime.AddHours(8);

            // Если время окончания работы больше 20:00, устанавливаем его на 20:00
            if (workEndTime.Hour >= 20)
            {
                workEndTime = new DateTime(currentLocalTime.Year, currentLocalTime.Month, currentLocalTime.Day, 20, 0, 0);
            }

            string timeWorked = $"{workStartTime:HH:mm} - {workEndTime:HH:mm}";
            TimeWorkedTextBlock.Text = $"Ваше текущее время работы: {timeWorked}";

            // Сообщение о времени работы
            MessageBox.Show($"Текущее время с ПК: {currentLocalTime}, Время начала работы: {workStartTime}, Время конца работы: {workEndTime}");
            MessageBox.Show("Вы успешно отметились!");

            // Установить статус на "Завершил работу" после окончания рабочего времени
            Timer timer = new Timer((timerState) =>
            {
                if (DateTime.Now >= workEndTime)
                {
                    timesheetDb.UpdateStatus("Завершил работу");

                    // Обновляем статус в UI
                    StatusTextBlock.Text = "Статус задачи: Завершил работу";

                    // Сообщение
                    MessageBox.Show("Рабочий день завершен. Статус обновлен.");

                    // Перезагружаем данные и обновляем DataGrid
                    LoadTimesheets();

                    // Скрываем TimeWorkedTextBlock после завершения рабочего дня
                    TimeWorkedTextBlock.Visibility = Visibility.Collapsed;
                }
            }, null, workEndTime - DateTime.Now, Timeout.InfiniteTimeSpan);
        }
    }

    public class GridConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataGrid grid && targetType == typeof(Thickness))
            {
                var headersPresenter = FindVisualChild<DataGridColumnHeadersPresenter>(grid);
                if (headersPresenter != null)
                    return new Thickness(0, -headersPresenter.ActualHeight, 0, 0);
            }
            return new Thickness();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;
                result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }

    public class TimesheetDisplay
    {
        public string Day { get; set; }
        public string January { get; set; }
        public string February { get; set; }
        public string March { get; set; }
        public string April { get; set; }
        public string May { get; set; }
        public string June { get; set; }
        public string July { get; set; }
        public string August { get; set; }
        public string September { get; set; }
        public string October { get; set; }
        public string November { get; set; }
        public string December { get; set; }

        public void SetWorkType(string month, string workType)
        {
            switch (month)
            {
                case "Январь": January = workType; break;
                case "Февраль": February = workType; break;
                case "Март": March = workType; break;
                case "Апрель": April = workType; break;
                case "Май": May = workType; break;
                case "Июнь": June = workType; break;
                case "Июль": July = workType; break;
                case "Август": August = workType; break;
                case "Сентябрь": September = workType; break;
                case "Октябрь": October = workType; break;
                case "Ноябрь": November = workType; break;
                case "Декабрь": December = workType; break;
            }
        }
    }
}
