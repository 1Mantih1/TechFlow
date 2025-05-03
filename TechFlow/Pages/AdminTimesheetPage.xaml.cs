using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    public partial class AdminTimesheetPage : Page
    {
        public ObservableCollection<WorkStatus> Statuses { get; } = new ObservableCollection<WorkStatus>();
        public ObservableCollection<User> Employees { get; } = new ObservableCollection<User>();
        public ObservableCollection<TimesheetDisplay> Timesheets { get; } = new ObservableCollection<TimesheetDisplay>();

        private readonly TimesheetFromDb _timesheetDb = new TimesheetFromDb();
        private readonly UserFromDb _employeeDb = new UserFromDb();

        public AdminTimesheetPage()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += OnPageLoaded;
            DataContext = this;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadStatuses();
                LoadEmployees();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при загрузке страницы: {ex.Message}");
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void LoadStatuses()
        {
            try
            {
                Statuses.Clear();
                var statuses = _timesheetDb.LoadWorkStatuses();
                foreach (var status in statuses)
                {
                    Statuses.Add(status);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки статусов: {ex.Message}");
            }
        }

        private void LoadEmployees()
        {
            try
            {
                Employees.Clear();
                var employees = _employeeDb.LoadEmployees();
                foreach (var employee in employees)
                {
                    // Добавьте проверку перед добавлением
                    if (string.IsNullOrEmpty(employee.FullName))
                    {
                        Console.WriteLine($"Empty FullName for user: {employee.UserId}");
                    }
                    Employees.Add(employee);
                }

                // Проверка после загрузки
                Console.WriteLine($"Loaded {Employees.Count} employees");
                foreach (var emp in Employees.Take(3))
                {
                    Console.WriteLine($"User: {emp.UserId}, FullName: {emp.FullName}");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EmployeesComboBox.SelectedItem == null)
                {
                    CustomMessageBox.Show("Выберите сотрудника для сохранения графика");
                    return;
                }

                User selectedEmployee = (User)EmployeesComboBox.SelectedItem;
                int employeeId = selectedEmployee.UserId;

                // Проверка данных перед сохранением
                if (Timesheets == null || Timesheets.Count == 0)
                {
                    CustomMessageBox.Show("Нет данных для сохранения");
                    return;
                }

                var newRecords = new List<TimesheetRecord>();
                foreach (var dayEntry in Timesheets)
                {
                    for (int month = 1; month <= 12; month++)
                    {
                        string statusCode = GetStatusCodeForMonth(dayEntry, month);
                        if (!string.IsNullOrEmpty(statusCode)) // Сохраняем только непустые статусы
                        {
                            newRecords.Add(new TimesheetRecord
                            {
                                EmployeeId = employeeId,
                                Day = int.Parse(dayEntry.Day),
                                Month = month,
                                StatusCode = statusCode
                            });
                        }
                    }
                }

                bool success = _timesheetDb.SaveTimesheetChanges(employeeId, newRecords);

                if (success)
                {
                    CustomMessageBox.Show("График работы успешно обновлен!");
                    LoadEmployeeTimesheet(employeeId); // Обновляем данные после сохранения
                }
                else
                {
                    CustomMessageBox.Show("Произошла ошибка при сохранении графика");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при сохранении графика: {ex.Message}");
            }
        }

        private string GetStatusCodeForMonth(TimesheetDisplay dayEntry, int month)
        {
            switch (month)
            {
                case 1: return dayEntry.January;
                case 2: return dayEntry.February;
                case 3: return dayEntry.March;
                case 4: return dayEntry.April;
                case 5: return dayEntry.May;
                case 6: return dayEntry.June;
                case 7: return dayEntry.July;
                case 8: return dayEntry.August;
                case 9: return dayEntry.September;
                case 10: return dayEntry.October;
                case 11: return dayEntry.November;
                case 12: return dayEntry.December;
                default: return null;
            }
        }

        private void EmployeesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeesComboBox.SelectedItem is User selectedEmployee)
            {
                LoadEmployeeTimesheet(selectedEmployee.UserId);
            }
        }

        private void LoadEmployeeTimesheet(int employeeId)
        {
            try
            {
                Timesheets.Clear();
                var timesheets = _timesheetDb.LoadEmployeeTimesheets(employeeId);

                for (int day = 1; day <= 31; day++)
                {
                    var dayEntry = new TimesheetDisplay { Day = day.ToString("00") };

                    // Fill in existing data
                    foreach (var record in timesheets.Where(t => t.WorkDate.Day == day))
                    {
                        dayEntry.SetWorkType(record.WorkDate.Month - 1, record.Status?.StatusCode ?? string.Empty);
                    }

                    Timesheets.Add(dayEntry);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки графика: {ex.Message}");
            }
        }
    }
}