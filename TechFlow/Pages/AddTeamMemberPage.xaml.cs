using System;
using System.Windows;
using System.Windows.Controls;
using TechFlow.Classes;
using TechFlow.Models;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace TechFlow.Pages
{
    public partial class AddTeamMemberPage : Page
    {
        private readonly TeamFromDb _teamFromDb = new TeamFromDb();
        private readonly UserFromDb _userFromDb = new UserFromDb();
        private readonly TeamEmployeeFromDb _teamEmployeeFromDb = new TeamEmployeeFromDb();
        private List<EmployeeRole> _employeeRoles;

        public AddTeamMemberPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Загрузка команд
                var teams = _teamFromDb.LoadTeams();
                TeamComboBox.ItemsSource = teams;

                // Загрузка сотрудников
                var employees = _userFromDb.LoadEmployeesWithDefaultRole();
                EmployeeComboBox.ItemsSource = employees.Select(e => new
                {
                    e.UserId,
                    FullName = $"{e.LastName} {e.FirstName}",
                    e.ImagePath
                }).ToList();

                // Загрузка ролей
                _employeeRoles = LoadEmployeeRoles();
                RoleComboBox.ItemsSource = _employeeRoles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<EmployeeRole> LoadEmployeeRoles()
        {
            var roles = new List<EmployeeRole>();

            try
            {
                using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connection.Open();
                    string sql = "SELECT * FROM employee_role ORDER BY employee_role_name";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roles.Add(new EmployeeRole
                            {
                                EmployeeRoleId = Convert.ToInt32(reader["employee_role_id"]),
                                EmployeeRoleName = reader["employee_role_name"].ToString(),
                                EmployeeRoleDescription = reader["employee_role_description"] != DBNull.Value ?
                                    reader["employee_role_description"].ToString() : string.Empty
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return roles;
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void AddTeamMemberButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация данных
                if (TeamComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите команду", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (EmployeeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите сотрудника", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (RoleComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите роль", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получение выбранных значений
                dynamic selectedTeam = TeamComboBox.SelectedItem;
                dynamic selectedEmployee = EmployeeComboBox.SelectedItem;
                var selectedRole = (EmployeeRole)RoleComboBox.SelectedItem;

                int teamId = selectedTeam.TeamId;
                int employeeId = selectedEmployee.UserId;
                int roleId = selectedRole.EmployeeRoleId;

                // Проверка, не состоит ли уже сотрудник в этой команде
                if (_teamEmployeeFromDb.IsEmployeeInTeam(employeeId, teamId))
                {
                    MessageBox.Show("Этот сотрудник уже состоит в выбранной команде", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Добавление сотрудника в команду
                bool success = _teamEmployeeFromDb.AddTeamEmployee(roleId, teamId, employeeId);

                if (success)
                {
                    MessageBox.Show("Сотрудник успешно добавлен в команду", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    // Возврат на предыдущую страницу
                    if (NavigationService.CanGoBack)
                    {
                        NavigationService.GoBack();
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось добавить сотрудника в команду", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении сотрудника: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class EmployeeRole
    {
        public int EmployeeRoleId { get; set; }
        public string EmployeeRoleName { get; set; }
        public string EmployeeRoleDescription { get; set; }
    }
}