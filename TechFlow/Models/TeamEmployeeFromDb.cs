using Npgsql;
using System;
using System.Collections.Generic;
using System.Windows;
using TechFlow.Classes;

namespace TechFlow.Models
{
    class TeamEmployeeFromDb
    {
        public List<TeamEmployee> LoadTeamEmployees(int teamId)
        {
            var members = new List<TeamEmployee>();

            try
            {
                using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connection.Open();

                    string sql = @"
                        SELECT te.team_employee_id, te.employee_role_id, te.team_id, te.employee_id, 
                               er.employee_role_name, t.team_name, 
                               e.first_name || ' ' || e.last_name AS employee_name,
                               e.image_path
                        FROM team_employee te
                        JOIN employee_role er ON te.employee_role_id = er.employee_role_id
                        JOIN team t ON te.team_id = t.team_id
                        JOIN employee e ON te.employee_id = e.employee_id
                        WHERE te.team_id = @teamId
                        ORDER BY employee_name";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@teamId", teamId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                members.Add(new TeamEmployee(
                                    Convert.ToInt32(reader["team_employee_id"]),
                                    Convert.ToInt32(reader["employee_role_id"]),
                                    Convert.ToInt32(reader["team_id"]),
                                    Convert.ToInt32(reader["employee_id"]),
                                    reader["employee_role_name"].ToString(),
                                    reader["team_name"].ToString(),
                                    reader["employee_name"].ToString(),
                                    reader["image_path"].ToString()
                                ));
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка загрузки участников команды: {ex.Message}");
            }

            return members;
        }

        public bool AddTeamEmployee(int employeeRoleId, int teamId, int employeeId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connection.Open();

                    string sql = @"
                        INSERT INTO team_employee (employee_role_id, team_id, employee_id)
                        VALUES (@roleId, @teamId, @employeeId)";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@roleId", employeeRoleId);
                        cmd.Parameters.AddWithValue("@teamId", teamId);
                        cmd.Parameters.AddWithValue("@employeeId", employeeId);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка добавления сотрудника в команду: {ex.Message}");
                return false;
            }
        }

        public bool IsEmployeeInTeam(int employeeId, int teamId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connection.Open();

                    string sql = @"
                        SELECT COUNT(*) 
                        FROM team_employee 
                        WHERE employee_id = @employeeId AND team_id = @teamId";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@employeeId", employeeId);
                        cmd.Parameters.AddWithValue("@teamId", teamId);

                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка проверки сотрудника в команде: {ex.Message}");
                return false;
            }
        }
    }
}

