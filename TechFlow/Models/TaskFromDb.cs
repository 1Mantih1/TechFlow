using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows;
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

                    bool isAdmin = CheckIfAdmin(connection, currentEmployeeId);
                    string sql = isAdmin
                        ? @"SELECT t.task_id, t.task_name, t.task_description, t.start_date, t.end_date,
                           t.status_id, s.status_name, t.stage_id, ps.stage_name, t.team_id, tm.team_name
                    FROM task t
                    JOIN status s ON t.status_id = s.status_id
                    JOIN project_stage ps ON t.stage_id = ps.stage_id
                    JOIN team tm ON t.team_id = tm.team_id"
                        : @"SELECT t.task_id, t.task_name, t.task_description, t.start_date, t.end_date,
                           t.status_id, s.status_name, t.stage_id, ps.stage_name, t.team_id, tm.team_name
                    FROM task t
                    JOIN status s ON t.status_id = s.status_id
                    JOIN project_stage ps ON t.stage_id = ps.stage_id
                    JOIN team tm ON t.team_id = tm.team_id
                    JOIN team_employee te ON tm.team_id = te.team_id
                    WHERE te.employee_id = @employee_id";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        if (!isAdmin)
                        {
                            command.Parameters.AddWithValue("@employee_id", currentEmployeeId);
                        }

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tasks.Add(new ProjectTask(
                                    reader.GetInt32(0),       
                                    reader.GetString(1),      
                                    reader.IsDBNull(2) ? string.Empty : reader.GetString(2), 
                                    reader.GetDateTime(3),   
                                    reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4), 
                                    reader.GetInt32(5),    
                                    reader.GetString(6),      
                                    reader.GetInt32(7),      
                                    reader.GetString(8),      
                                    reader.GetInt32(9),     
                                    reader.GetString(10)     
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    ShowErrorMessage("Ошибка загрузки задач", ex.Message);
                }
            }

            return tasks;
        }


        public List<ProjectTask> SearchTasks(string searchText)
        {
            List<ProjectTask> tasks = new List<ProjectTask>();
            int currentEmployeeId = Authorization.currentUser.UserId;

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    bool isAdmin = CheckIfAdmin(connection, currentEmployeeId);
                    string sql = BuildSearchQuery(isAdmin);

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@search", $"%{searchText}%");
                        if (!isAdmin)
                        {
                            command.Parameters.AddWithValue("@employeeId", currentEmployeeId);
                        }

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tasks.Add(CreateTaskFromReader(reader));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    ShowErrorMessage("Ошибка поиска задач", ex.Message);
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
            int currentEmployeeId = Authorization.currentUser.UserId;

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    bool isAdmin = CheckIfAdmin(connection, currentEmployeeId);
                    var (sql, parameters) = BuildFilterQuery(
                        searchText, taskSearchText, stageSearchText,
                        status, stage, dateFilterOption, isUrgent,
                        sortBy, isAdmin, currentEmployeeId);

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tasks.Add(CreateTaskFromReader(reader));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    ShowErrorMessage("Ошибка фильтрации задач", ex.Message);
                }
            }

            return tasks;
        }

        public ProjectTask GetTaskById(int taskId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM fn_get_task_by_id(@p_task_id)", connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@p_task_id", taskId);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return CreateTaskFromReader(reader);
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    ShowErrorMessage("Ошибка при загрузке задачи", ex.Message);
                    return null;
                }
            }
            return null;
        }

        public int CountTaskParticipants(int taskId)
        {
            int count = 0;

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(
                        @"SELECT COUNT(*) 
                          FROM team_employee te
                          JOIN task t ON te.team_id = t.team_id
                          WHERE t.task_id = @taskId", connection))
                    {
                        command.Parameters.AddWithValue("@taskId", taskId);
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
                catch (NpgsqlException ex)
                {
                    ShowErrorMessage("Ошибка подсчета участников задачи", ex.Message);
                }
            }

            return count;
        }

        public bool UpdateTaskStatus(int taskId, int statusId)
        {
            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand("UPDATE task SET status_id = @p_status_id WHERE task_id = @p_task_id", connection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@p_task_id", taskId);
                        cmd.Parameters.AddWithValue("@p_status_id", statusId);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (NpgsqlException ex)
                {
                    ShowErrorMessage("Ошибка при обновлении статуса задачи", ex.Message);
                    return false;
                }
            }
        }

        private bool CheckIfAdmin(NpgsqlConnection connection, int employeeId)
        {
            const string checkAdminSql = @"
                SELECT er.employee_role_name 
                FROM employee e 
                JOIN employee_role er ON e.role_id = er.employee_role_id 
                WHERE e.employee_id = @employeeId";

            using (NpgsqlCommand adminCheckCmd = new NpgsqlCommand(checkAdminSql, connection))
            {
                adminCheckCmd.Parameters.AddWithValue("@employeeId", employeeId);
                string roleName = adminCheckCmd.ExecuteScalar()?.ToString();
                return roleName == "Администратор";
            }
        }

        private string BuildSearchQuery(bool isAdmin)
        {
            return @"
                SELECT t.task_id, t.task_name, t.task_description, t.start_date, t.end_date, 
                       t.status_id, t.stage_id, t.team_id, 
                       s.status_name, ps.stage_name, tm.team_name
                FROM task t
                INNER JOIN status s ON t.status_id = s.status_id
                INNER JOIN project_stage ps ON t.stage_id = ps.stage_id
                INNER JOIN team tm ON t.team_id = tm.team_id
                " + (isAdmin ? "" : @"
                INNER JOIN team_employee te ON tm.team_id = te.team_id
                ") + @"
                WHERE t.task_name ILIKE @search
                " + (isAdmin ? "" : "AND te.employee_id = @employeeId") + @"
                ORDER BY t.task_id;";
        }

        private (string sql, List<NpgsqlParameter> parameters) BuildFilterQuery(
            string searchText,
            string taskSearchText,
            string stageSearchText,
            string status,
            string stage,
            string dateFilterOption,
            bool isUrgent,
            string sortBy,
            bool isAdmin,
            int currentEmployeeId)
        {
            string sql = @"
                SELECT DISTINCT t.task_id, t.task_name, t.task_description, t.start_date, t.end_date, 
                       t.status_id, t.stage_id, t.team_id, 
                       s.status_name, ps.stage_name, tm.team_name
                FROM task t
                INNER JOIN status s ON t.status_id = s.status_id
                INNER JOIN project_stage ps ON t.stage_id = ps.stage_id
                INNER JOIN team tm ON t.team_id = tm.team_id";

            if (!isAdmin)
            {
                sql += @"
                    INNER JOIN team_employee te ON tm.team_id = te.team_id";
            }

            var conditions = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            if (!isAdmin)
            {
                conditions.Add("te.employee_id = @employeeId");
                parameters.Add(new NpgsqlParameter("@employeeId", currentEmployeeId));
            }

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

            sql += GetOrderByClause(sortBy);

            return (sql, parameters);
        }

        private string GetOrderByClause(string sortBy)
        {
            switch (sortBy)
            {
                case "endingSoon":
                    return " ORDER BY CASE WHEN t.end_date IS NULL THEN 1 ELSE 0 END, t.end_date ASC";
                case "newest":
                    return " ORDER BY t.start_date DESC";
                case "urgent":
                    return " ORDER BY t.end_date ASC";
                default:
                    return " ORDER BY t.task_id";
            }
        }

        private ProjectTask CreateTaskFromReader(NpgsqlDataReader reader)
        {
            return new ProjectTask(
                taskId: reader.GetInt32(0),
                taskName: reader.GetString(1),
                taskDescription: reader.IsDBNull(2) ? "" : reader.GetString(2),
                startDate: reader.GetDateTime(3),
                endDate: reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                statusId: reader.GetInt32(5),
                statusName: reader.GetString(8),
                stageId: reader.GetInt32(6),
                stageName: reader.GetString(9),
                teamId: reader.GetInt32(7),
                teamName: reader.GetString(10)
            );
        }

        private void ShowErrorMessage(string title, string message)
        {
            MessageBox.Show($"{title}: {message}",
                          "Ошибка базы данных",
                          MessageBoxButton.OK,
                          MessageBoxImage.Error);
        }
    }
}