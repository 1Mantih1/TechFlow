using System;
using System.Windows;
using System.Windows.Controls;
using TechFlow.Models;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using TechFlow.Windows;

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
            TeamComboBox.SelectionChanged += TeamComboBox_SelectionChanged;
        }

        private void LoadData()
        {
            try
            {
                var teams = _teamFromDb.LoadTeams();
                TeamComboBox.ItemsSource = teams;

                _employeeRoles = LoadEmployeeRoles();
                RoleComboBox.ItemsSource = _employeeRoles;

                // Загружаем сотрудников с учетом выбранной команды (если есть)
                if (TeamComboBox.SelectedItem != null)
                {
                    var selectedTeam = (dynamic)TeamComboBox.SelectedItem;
                    RefreshEmployeeList(selectedTeam.TeamId);
                }
                else
                {
                    // Если команда не выбрана, показываем всех сотрудников
                    var employees = _userFromDb.LoadEmployeesWithRole()
                        .Select(e => new
                        {
                            e.UserId,
                            FullName = $"{e.LastName} {e.FirstName}",
                            e.ImagePath
                        })
                        .ToList();
                    EmployeeComboBox.ItemsSource = employees;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
        }

        private void RefreshEmployeeList(int teamId)
        {
            try
            {
                // Загружаем всех сотрудников
                var allEmployees = _userFromDb.LoadEmployeesWithRole();

                // Получаем ID сотрудников уже в этой команде
                var teamMembers = _teamEmployeeFromDb.LoadTeamEmployees(teamId)
                                    .Select(m => m.EmployeeId)
                                    .ToList();

                // Фильтруем - оставляем только тех, кто не в команде
                var availableEmployees = allEmployees
                    .Where(e => !teamMembers.Contains(e.UserId))
                    .Select(e => new
                    {
                        e.UserId,
                        FullName = $"{e.LastName} {e.FirstName}",
                        e.ImagePath
                    })
                    .ToList();

                // Обновляем комбобокс
                EmployeeComboBox.ItemsSource = availableEmployees;
                EmployeeComboBox.SelectedItem = null;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка обновления списка сотрудников: {ex.Message}", "Ошибка");
            }
        }

        private void TeamComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TeamComboBox.SelectedItem != null)
            {
                var selectedTeam = (dynamic)TeamComboBox.SelectedItem;
                RefreshEmployeeList(selectedTeam.TeamId);
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
                CustomMessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка");
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
                if (TeamComboBox.SelectedItem == null ||
                    EmployeeComboBox.SelectedItem == null ||
                    RoleComboBox.SelectedItem == null)
                {
                    CustomMessageBox.Show("Заполните все поля", "Ошибка");
                    return;
                }

                var selectedTeam = (dynamic)TeamComboBox.SelectedItem;
                var selectedEmployee = (dynamic)EmployeeComboBox.SelectedItem;
                var selectedRole = (EmployeeRole)RoleComboBox.SelectedItem;

                int teamId = selectedTeam.TeamId;
                int employeeId = selectedEmployee.UserId;
                int roleId = selectedRole.EmployeeRoleId;

                bool isInTeam = _teamEmployeeFromDb.IsEmployeeInTeam(employeeId, teamId);

                if (isInTeam)
                {
                    bool updateSuccess = _teamEmployeeFromDb.UpdateTeamMemberRole(employeeId, teamId, roleId);

                    if (updateSuccess)
                    {
                        CustomMessageBox.Show("Роль сотрудника обновлена", "Успех");
                        NavigationService.GoBack();
                    }
                    else
                    {
                        CustomMessageBox.Show("Не удалось обновить роль", "Ошибка");
                    }
                }
                else
                {
                    bool addSuccess = _teamEmployeeFromDb.AddTeamEmployee(roleId, teamId, employeeId);

                    if (addSuccess)
                    {
                        // Обновляем список сотрудников после добавления
                        RefreshEmployeeList(teamId);
                        CustomMessageBox.Show("Сотрудник добавлен в команду", "Успех");
                        NavigationService.GoBack();
                    }
                    else
                    {
                        CustomMessageBox.Show("Не удалось добавить сотрудника", "Ошибка");
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
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