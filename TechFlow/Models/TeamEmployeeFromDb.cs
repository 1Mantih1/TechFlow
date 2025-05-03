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

                    string sql = "SELECT * FROM fn_get_team_members(@teamId)";

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
                VALUES (@roleId, @teamId, @employeeId)
                RETURNING team_employee_id";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@roleId", employeeRoleId);
                        cmd.Parameters.AddWithValue("@teamId", teamId);
                        cmd.Parameters.AddWithValue("@employeeId", employeeId);

                        var result = cmd.ExecuteScalar();
                        return result != null && result != DBNull.Value;
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка добавления сотрудника в команду: {ex.Message}\nПодробности: {ex.InnerException?.Message}");
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

                    string sql = "SELECT fn_is_employee_in_team(@employeeId, @teamId)";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@employeeId", employeeId);
                        cmd.Parameters.AddWithValue("@teamId", teamId);

                        return (bool)cmd.ExecuteScalar();
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка проверки сотрудника в команде: {ex.Message}");
                return false;
            }
        }

        public bool RemoveTeamMember(int teamEmployeeId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connection.Open();

                    string sql = "SELECT fn_remove_team_member(@teamEmployeeId)";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@teamEmployeeId", teamEmployeeId);

                        return (bool)cmd.ExecuteScalar();
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка удаления участника из команды: {ex.Message}");
                return false;
            }
        }

        public bool UpdateTeamMemberRole(int employeeId, int teamId, int newRoleId)
        {
            Console.WriteLine($"UpdateTeamMemberRole called with: employeeId={employeeId}, teamId={teamId}, newRoleId={newRoleId}");

            try
            {
                using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 1. Проверяем существование записи
                            string checkSql = @"
                        SELECT team_employee_id, employee_role_id 
                        FROM team_employee 
                        WHERE team_id = @teamId AND employee_id = @employeeId
                        FOR UPDATE";

                            int currentRoleId = -1;
                            int teamEmployeeId = -1;

                            using (var checkCmd = new NpgsqlCommand(checkSql, connection, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@teamId", teamId);
                                checkCmd.Parameters.AddWithValue("@employeeId", employeeId);

                                using (var reader = checkCmd.ExecuteReader())
                                {
                                    if (!reader.Read())
                                    {
                                        MessageBox.Show("Сотрудник не найден в указанной команде", "Ошибка");
                                        return false;
                                    }

                                    teamEmployeeId = reader.GetInt32(0);
                                    currentRoleId = reader.GetInt32(1);
                                }
                            }

                            Console.WriteLine($"Current role: {currentRoleId}, TeamEmployeeId: {teamEmployeeId}");

                            // 2. Если роль не меняется — возвращаемся
                            if (currentRoleId == newRoleId)
                            {
                                MessageBox.Show("Сотрудник уже имеет указанную роль", "Информация");
                                return true;
                            }

                            // 3. Проверяем, существует ли новая роль
                            string checkRoleSql = "SELECT 1 FROM employee_role WHERE employee_role_id = @newRoleId";
                            using (var checkRoleCmd = new NpgsqlCommand(checkRoleSql, connection, transaction))
                            {
                                checkRoleCmd.Parameters.AddWithValue("@newRoleId", newRoleId);
                                if (checkRoleCmd.ExecuteScalar() == null)
                                {
                                    MessageBox.Show("Указанная роль не существует", "Ошибка");
                                    return false;
                                }
                            }

                            // 4. Обновляем роль
                            string updateSql = @"
                        UPDATE team_employee 
                        SET employee_role_id = @newRoleId 
                        WHERE team_employee_id = @teamEmployeeId";

                            using (var updateCmd = new NpgsqlCommand(updateSql, connection, transaction))
                            {
                                updateCmd.Parameters.AddWithValue("@newRoleId", newRoleId);
                                updateCmd.Parameters.AddWithValue("@teamEmployeeId", teamEmployeeId);

                                int rowsAffected = updateCmd.ExecuteNonQuery();
                                if (rowsAffected == 0)
                                {
                                    MessageBox.Show("Не удалось обновить роль", "Ошибка");
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            transaction.Commit();
                            Console.WriteLine("Роль успешно обновлена");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Ошибка в транзакции: {ex.Message}\n{ex.StackTrace}");
                            throw;
                        }
                    }
                }
            }
            catch (PostgresException pgEx)
            {
                Console.WriteLine($"Postgres ошибка: {pgEx.MessageText}");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления роли: {ex.Message}", "Ошибка");
                Console.WriteLine($"Общая ошибка: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }
    }
}