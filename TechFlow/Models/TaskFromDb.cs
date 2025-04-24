using Npgsql;
using System.Collections.Generic;
using System.Windows;
using System;
using TechFlow.Classes;
using TechFlow.Windows;

namespace TechFlow.Models
{
    class TaskFromDb
    {
        public List<ProjectTask> LoadTasks()
        {
            List<ProjectTask> tasks = new List<ProjectTask>();
            int currentEmployeeId = Authorization.currentUser.UserId;

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    bool isAdmin = false;
                    string checkAdminSql = "SELECT er.employee_role_name FROM employee e " +
                                           "JOIN employee_role er ON e.role_id = er.employee_role_id " +
                                           "WHERE e.employee_id = @employeeId";

                    using (NpgsqlCommand adminCheckCmd = new NpgsqlCommand(checkAdminSql, connection))
                    {
                        adminCheckCmd.Parameters.AddWithValue("@employeeId", currentEmployeeId);
                        string roleName = adminCheckCmd.ExecuteScalar()?.ToString();
                        isAdmin = roleName == "Администратор";
                    }

                    string sqlExp = @"
                SELECT t.task_id, t.task_name, t.task_description, t.start_date, t.end_date, 
                       t.status_id, t.stage_id, t.team_id, 
                       s.status_name, ps.stage_name, tm.team_name
                FROM public.task t
                INNER JOIN public.status s ON t.status_id = s.status_id
                INNER JOIN public.project_stage ps ON t.stage_id = ps.stage_id
                INNER JOIN public.team tm ON t.team_id = tm.team_id " +
                        (isAdmin ? "" : "WHERE t.task_id IN (" +
                            "SELECT t.task_id FROM task t " +
                            "JOIN team_employee te ON t.team_id = te.team_id " +
                            "WHERE te.employee_id = @employeeId)") +
                        " ORDER BY t.task_id;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    {
                        if (!isAdmin)
                        {
                            command.Parameters.AddWithValue("@employeeId", currentEmployeeId);
                        }

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tasks.Add(new ProjectTask(
                                    Convert.ToInt32(reader["task_id"]),
                                    reader["task_name"].ToString(),
                                    reader["task_description"]?.ToString() ?? "",
                                    Convert.ToDateTime(reader["start_date"]),
                                    reader["end_date"] != DBNull.Value ? Convert.ToDateTime(reader["end_date"]) : (DateTime?)null,
                                    Convert.ToInt32(reader["status_id"]),
                                    reader["status_name"].ToString(),
                                    Convert.ToInt32(reader["stage_id"]),
                                    reader["stage_name"].ToString(),
                                    Convert.ToInt32(reader["team_id"]),
                                    reader["team_name"].ToString()
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки задач: " + ex.Message);
                }
            }
            return tasks;
        }

        public List<ProjectTask> SearchTasks(string searchText)
        {
            List<ProjectTask> tasks = new List<ProjectTask>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sql = @"
                SELECT t.task_id, t.task_name, t.task_description, t.start_date, t.end_date, 
                       t.status_id, t.stage_id, t.team_id, 
                       s.status_name, ps.stage_name, tm.team_name
                FROM public.task t
                INNER JOIN public.status s ON t.status_id = s.status_id
                INNER JOIN public.project_stage ps ON t.stage_id = ps.stage_id
                INNER JOIN public.team tm ON t.team_id = tm.team_id
                WHERE t.task_name ILIKE @search
                ORDER BY t.task_id";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@search", $"%{searchText}%");

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tasks.Add(new ProjectTask(
                                    Convert.ToInt32(reader["task_id"]),
                                    reader["task_name"].ToString(),
                                    reader["task_description"]?.ToString() ?? "",
                                    Convert.ToDateTime(reader["start_date"]),
                                    reader["end_date"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["end_date"])
                                        : (DateTime?)null,
                                    Convert.ToInt32(reader["status_id"]),
                                    reader["status_name"].ToString(),
                                    Convert.ToInt32(reader["stage_id"]),
                                    reader["stage_name"].ToString(),
                                    Convert.ToInt32(reader["team_id"]),
                                    reader["team_name"].ToString()
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка поиска задач: {ex.Message}",
                                  "Ошибка базы данных",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
            return tasks;
        }

        public List<ProjectTask> FilterTasks(
            string searchText,
            string taskSearchText,
            string stageSearchText,
            string status,
            string stage,
            string dateFilterOption,
            bool isUrgent,
            string sortBy = "default")
        {
            List<ProjectTask> tasks = new List<ProjectTask>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    string sql = @"
                SELECT t.task_id, t.task_name, t.task_description, t.start_date, t.end_date, 
                       t.status_id, t.stage_id, t.team_id, 
                       s.status_name, ps.stage_name, tm.team_name
                FROM public.task t
                INNER JOIN public.status s ON t.status_id = s.status_id
                INNER JOIN public.project_stage ps ON t.stage_id = ps.stage_id
                INNER JOIN public.team tm ON t.team_id = tm.team_id";

                    var conditions = new List<string>();
                    var parameters = new List<NpgsqlParameter>();

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        conditions.Add("t.task_name ILIKE @search");
                        parameters.Add(new NpgsqlParameter("@search", $"%{searchText}%"));
                    }

                    if (!string.IsNullOrEmpty(taskSearchText))
                    {
                        conditions.Add("t.task_description ILIKE @taskSearch");
                        parameters.Add(new NpgsqlParameter("@taskSearch", $"%{taskSearchText}%"));
                    }

                    if (!string.IsNullOrEmpty(stageSearchText))
                    {
                        conditions.Add("ps.stage_name ILIKE @stageSearch");
                        parameters.Add(new NpgsqlParameter("@stageSearch", $"%{stageSearchText}%"));
                    }

                    if (!string.IsNullOrEmpty(status) && status != "Все")
                    {
                        conditions.Add("s.status_name = @status");
                        parameters.Add(new NpgsqlParameter("@status", status));
                    }

                    if (!string.IsNullOrEmpty(stage) && stage != "Все этапы")
                    {
                        conditions.Add("ps.stage_name = @stage");
                        parameters.Add(new NpgsqlParameter("@stage", stage));
                    }

                    if (!string.IsNullOrWhiteSpace(dateFilterOption) && dateFilterOption != "Любая дата")
                    {
                        switch (dateFilterOption)
                        {
                            case "Сегодня":
                                conditions.Add("t.start_date::date = CURRENT_DATE");
                                break;
                            case "На этой неделе":
                                conditions.Add("t.start_date >= date_trunc('week', CURRENT_DATE)");
                                break;
                            case "В этом месяце":
                                conditions.Add("t.start_date >= date_trunc('month', CURRENT_DATE)");
                                break;
                            case "Просроченные":
                                conditions.Add("t.end_date IS NOT NULL AND t.end_date < CURRENT_DATE");
                                break;
                        }
                    }

                    if (isUrgent)
                    {
                        conditions.Add("t.end_date IS NOT NULL AND t.end_date <= CURRENT_DATE + INTERVAL '3 days'");
                    }

                    if (conditions.Count > 0)
                    {
                        sql += " WHERE " + string.Join(" AND ", conditions);
                    }

                    switch (sortBy)
                    {
                        case "endingSoon":
                            sql += " ORDER BY CASE WHEN t.end_date IS NULL THEN 1 ELSE 0 END, t.end_date ASC";
                            break;
                        case "newest":
                            sql += " ORDER BY t.start_date DESC";
                            break;
                        case "urgent":
                            sql += " ORDER BY t.end_date ASC";
                            break;
                        default:
                            sql += " ORDER BY t.task_id";
                            break;
                    }

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tasks.Add(new ProjectTask(
                                    Convert.ToInt32(reader["task_id"]),
                                    reader["task_name"].ToString(),
                                    reader["task_description"]?.ToString() ?? "",
                                    Convert.ToDateTime(reader["start_date"]),
                                    reader["end_date"] != DBNull.Value ? Convert.ToDateTime(reader["end_date"]) : (DateTime?)null,
                                    Convert.ToInt32(reader["status_id"]),
                                    reader["status_name"].ToString(),
                                    Convert.ToInt32(reader["stage_id"]),
                                    reader["stage_name"].ToString(),
                                    Convert.ToInt32(reader["team_id"]),
                                    reader["team_name"].ToString()
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка фильтрации задач: " + ex.Message);
                }
            }

            return tasks;
        }

        public int CountTaskParticipants(int taskId)
        {
            int count = 0;

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    string sql = @"
                SELECT COUNT(DISTINCT te.employee_id) 
                FROM task t
                JOIN team_employee te ON t.team_id = te.team_id
                WHERE t.task_id = @taskId";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@taskId", taskId);
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка подсчета участников задачи: " + ex.Message);
                }
            }

            return count;
        }
    }
}
