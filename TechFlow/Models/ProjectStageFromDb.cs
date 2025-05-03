using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using TechFlow.Classes;
using TechFlow.Windows;

namespace TechFlow.Models
{
    class ProjectStageFromDb
    {
        private bool IsCurrentUserAdmin(NpgsqlConnection connection)
        {
            int currentEmployeeId = Authorization.currentUser.UserId;

            using (NpgsqlCommand adminCheckCmd = new NpgsqlCommand(
                "SELECT er.employee_role_name FROM employee e " +
                "JOIN employee_role er ON e.role_id = er.employee_role_id " +
                "WHERE e.employee_id = @employeeId", connection))
            {
                adminCheckCmd.Parameters.AddWithValue("@employeeId", currentEmployeeId);
                string roleName = adminCheckCmd.ExecuteScalar()?.ToString();
                return roleName == "Администратор";
            }
        }

        public List<ProjectStage> LoadProjectStages()
        {
            List<ProjectStage> projectStages = new List<ProjectStage>();
            int currentEmployeeId = Authorization.currentUser.UserId;

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    bool isAdmin = IsCurrentUserAdmin(connection);

                    string sql = @"
                        SELECT ps.stage_id, ps.stage_name, ps.project_stage_description, 
                               ps.start_date, ps.end_date, s.status_name, 
                               p.project_id, p.project_name
                        FROM project_stage ps
                        JOIN status s ON ps.status_id = s.status_id
                        JOIN project p ON ps.project_id = p.project_id";

                    if (!isAdmin)
                    {
                        sql += @"
                        JOIN task t ON t.stage_id = ps.stage_id
                        JOIN team_employee te ON t.team_id = te.team_id AND te.employee_id = @employeeId";
                    }

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        if (!isAdmin)
                        {
                            command.Parameters.AddWithValue("@employeeId", currentEmployeeId);
                        }

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                projectStages.Add(new ProjectStage(
                                    stageId: reader.GetInt32(reader.GetOrdinal("stage_id")),
                                    stageName: reader.GetString(reader.GetOrdinal("stage_name")),
                                    projectStageDescription: reader.IsDBNull(reader.GetOrdinal("project_stage_description")) ? "" : reader.GetString(reader.GetOrdinal("project_stage_description")),
                                    startDate: reader.GetDateTime(reader.GetOrdinal("start_date")),
                                    endDate: reader.IsDBNull(reader.GetOrdinal("end_date")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("end_date")),
                                    status: reader.GetString(reader.GetOrdinal("status_name")),
                                    projectId: reader.GetInt32(reader.GetOrdinal("project_id")),
                                    projectName: reader.GetString(reader.GetOrdinal("project_name"))
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки стадий проекта: " + ex.Message,
                                  "Ошибка базы данных",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
            return projectStages;
        }

        public List<ProjectStage> SearchProjectStages(string searchText)
        {
            List<ProjectStage> projectStages = new List<ProjectStage>();
            int currentEmployeeId = Authorization.currentUser.UserId;

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    bool isAdmin = IsCurrentUserAdmin(connection);

                    string sql = @"
                        SELECT ps.stage_id, ps.stage_name, ps.project_stage_description, 
                               ps.start_date, ps.end_date, s.status_name, 
                               p.project_id, p.project_name
                        FROM project_stage ps
                        JOIN status s ON ps.status_id = s.status_id
                        JOIN project p ON ps.project_id = p.project_id
                        WHERE ps.stage_name ILIKE @search";

                    if (!isAdmin)
                    {
                        sql += @"
                        AND EXISTS (
                            SELECT 1 FROM task t
                            JOIN team_employee te ON t.team_id = te.team_id
                            WHERE t.stage_id = ps.stage_id AND te.employee_id = @employeeId
                        )";
                    }

                    sql += " ORDER BY ps.stage_id;";

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
                                projectStages.Add(new ProjectStage(
                                    Convert.ToInt32(reader["stage_id"]),
                                    reader["stage_name"].ToString(),
                                    reader["project_stage_description"]?.ToString() ?? "",
                                    Convert.ToDateTime(reader["start_date"]),
                                    reader["end_date"] != DBNull.Value ? Convert.ToDateTime(reader["end_date"]) : (DateTime?)null,
                                    reader["status_name"].ToString(),
                                    Convert.ToInt32(reader["project_id"]),
                                    reader["project_name"].ToString()
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка поиска этапов проекта: " + ex.Message);
                }
            }
            return projectStages;
        }

        public List<ProjectStage> FilterProjectStages(
            string searchText,
            string projectName,
            string status,
            string dateFilterOption,
            bool isUrgent,
            string sortBy = "default")
        {
            List<ProjectStage> projectStages = new List<ProjectStage>();
            int currentEmployeeId = Authorization.currentUser.UserId;

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    bool isAdmin = IsCurrentUserAdmin(connection);

                    string sql = @"
                        SELECT ps.stage_id, ps.stage_name, ps.project_stage_description, 
                               ps.start_date, ps.end_date, s.status_name, 
                               p.project_id, p.project_name
                        FROM project_stage ps
                        JOIN status s ON ps.status_id = s.status_id
                        JOIN project p ON ps.project_id = p.project_id
                        WHERE 1=1";

                    if (!isAdmin)
                    {
                        sql += @"
                        AND EXISTS (
                            SELECT 1 FROM task t
                            JOIN team_employee te ON t.team_id = te.team_id
                            WHERE t.stage_id = ps.stage_id AND te.employee_id = @employeeId
                        )";
                    }

                    var parameters = new List<NpgsqlParameter>();

                    if (!isAdmin)
                    {
                        parameters.Add(new NpgsqlParameter("@employeeId", currentEmployeeId));
                    }

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        sql += " AND ps.stage_name ILIKE @search";
                        parameters.Add(new NpgsqlParameter("@search", $"%{searchText}%"));
                    }

                    if (!string.IsNullOrEmpty(projectName))
                    {
                        sql += " AND p.project_name ILIKE @projectName";
                        parameters.Add(new NpgsqlParameter("@projectName", $"%{projectName}%"));
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        sql += " AND s.status_name = @status";
                        parameters.Add(new NpgsqlParameter("@status", status));
                    }

                    if (!string.IsNullOrWhiteSpace(dateFilterOption))
                    {
                        switch (dateFilterOption)
                        {
                            case "Сегодня":
                                sql += " AND ps.start_date::date = CURRENT_DATE";
                                break;
                            case "На этой неделе":
                                sql += " AND ps.start_date >= date_trunc('week', CURRENT_DATE)";
                                break;
                            case "В этом месяце":
                                sql += " AND ps.start_date >= date_trunc('month', CURRENT_DATE)";
                                break;
                            case "Просроченные":
                                sql += " AND ps.end_date IS NOT NULL AND ps.end_date < CURRENT_DATE";
                                break;
                        }
                    }

                    if (isUrgent)
                    {
                        sql += " AND ps.end_date IS NOT NULL AND ps.end_date <= CURRENT_DATE + INTERVAL '3 days'";
                    }

                    switch (sortBy)
                    {
                        case "endingSoon":
                            sql += " ORDER BY CASE WHEN ps.end_date IS NULL THEN 1 ELSE 0 END, ps.end_date ASC";
                            break;
                        case "newest":
                            sql += " ORDER BY ps.start_date DESC";
                            break;
                        case "urgent":
                            sql += " ORDER BY ps.end_date ASC";
                            break;
                        default:
                            sql += " ORDER BY ps.stage_id";
                            break;
                    }

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                projectStages.Add(new ProjectStage(
                                    Convert.ToInt32(reader["stage_id"]),
                                    reader["stage_name"].ToString(),
                                    reader["project_stage_description"]?.ToString() ?? "",
                                    Convert.ToDateTime(reader["start_date"]),
                                    reader["end_date"] != DBNull.Value ? Convert.ToDateTime(reader["end_date"]) : (DateTime?)null,
                                    reader["status_name"].ToString(),
                                    Convert.ToInt32(reader["project_id"]),
                                    reader["project_name"].ToString()
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка фильтрации стадий проекта: " + ex.Message);
                }
            }

            return projectStages;
        }

        public ProjectStage GetProjectStageById(int stageId)
        {
            int currentEmployeeId = Authorization.currentUser.UserId;

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    bool isAdmin = IsCurrentUserAdmin(connection);

                    string sql = @"
                        SELECT ps.stage_id, ps.stage_name, ps.project_stage_description, 
                               ps.start_date, ps.end_date, s.status_name, 
                               p.project_id, p.project_name
                        FROM project_stage ps
                        JOIN status s ON ps.status_id = s.status_id
                        JOIN project p ON ps.project_id = p.project_id
                        WHERE ps.stage_id = @stageId";

                    if (!isAdmin)
                    {
                        sql += @"
                            AND EXISTS (
                                SELECT 1 FROM task t
                                JOIN team_employee te ON t.team_id = te.team_id
                                WHERE t.stage_id = ps.stage_id 
                                AND te.employee_id = @employeeId
                            )";
                    }

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@stageId", stageId);

                        if (!isAdmin)
                        {
                            command.Parameters.AddWithValue("@employeeId", currentEmployeeId);
                        }

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new ProjectStage(
                                    stageId: reader.GetInt32(reader.GetOrdinal("stage_id")),
                                    stageName: reader.GetString(reader.GetOrdinal("stage_name")),
                                    projectStageDescription: reader.IsDBNull(reader.GetOrdinal("project_stage_description")) ? "" : reader.GetString(reader.GetOrdinal("project_stage_description")),
                                    startDate: reader.GetDateTime(reader.GetOrdinal("start_date")),
                                    endDate: reader.IsDBNull(reader.GetOrdinal("end_date")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("end_date")),
                                    status: reader.GetString(reader.GetOrdinal("status_name")),
                                    projectId: reader.GetInt32(reader.GetOrdinal("project_id")),
                                    projectName: reader.GetString(reader.GetOrdinal("project_name"))
                                );
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка при загрузке стадии проекта: {ex.Message}",
                                  "Ошибка базы данных",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
            return null;
        }

        public bool CreateProjectStage(
            string stageName,
            string description,
            DateTime startDate,
            DateTime? endDate,
            int statusId,
            int projectId,
            out int newStageId)
        {
            newStageId = 0;
            int currentEmployeeId = Authorization.currentUser.UserId;

            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    bool isAdmin = IsCurrentUserAdmin(connection);

                    if (!isAdmin)
                    {
                        using (var checkCmd = new NpgsqlCommand(
                            @"SELECT COUNT(*) 
                              FROM task t
                              JOIN team_employee te ON t.team_id = te.team_id
                              WHERE t.project_id = @projectId AND te.employee_id = @employeeId",
                            connection))
                        {
                            checkCmd.Parameters.AddWithValue("@projectId", projectId);
                            checkCmd.Parameters.AddWithValue("@employeeId", currentEmployeeId);
                            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (count == 0)
                            {
                                MessageBox.Show("Вы не являетесь участником этого проекта и не можете создавать этапы для него",
                                              "Ошибка доступа",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Warning);
                                return false;
                            }
                        }
                    }

                    using (var cmd = new NpgsqlCommand(
                        "INSERT INTO project_stage (stage_name, project_stage_description, start_date, end_date, status_id, project_id) " +
                        "VALUES (@stageName, @description, @startDate, @endDate, @statusId, @projectId) RETURNING stage_id",
                        connection))
                    {
                        cmd.Parameters.AddWithValue("@stageName", stageName);
                        cmd.Parameters.AddWithValue("@description", description ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@statusId", statusId);
                        cmd.Parameters.AddWithValue("@projectId", projectId);

                        newStageId = Convert.ToInt32(cmd.ExecuteScalar());
                        return true;
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка при создании стадии проекта: {ex.Message}",
                                  "Ошибка базы данных",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                    return false;
                }
            }
        }

        public bool UpdateProjectStage(
            int stageId,
            string stageName,
            string description,
            DateTime startDate,
            DateTime? endDate,
            int statusId,
            int projectId)
        {
            int currentEmployeeId = Authorization.currentUser.UserId;

            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    bool isAdmin = IsCurrentUserAdmin(connection);

                    if (!isAdmin)
                    {
                        using (var checkCmd = new NpgsqlCommand(
                            @"SELECT COUNT(*) 
                              FROM task t
                              JOIN team_employee te ON t.team_id = te.team_id
                              WHERE t.stage_id = @stageId AND te.employee_id = @employeeId",
                            connection))
                        {
                            checkCmd.Parameters.AddWithValue("@stageId", stageId);
                            checkCmd.Parameters.AddWithValue("@employeeId", currentEmployeeId);
                            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (count == 0)
                            {
                                MessageBox.Show("У вас нет прав для редактирования этого этапа проекта",
                                              "Ошибка доступа",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Warning);
                                return false;
                            }
                        }
                    }

                    using (var cmd = new NpgsqlCommand(
                        @"UPDATE project_stage 
                          SET stage_name = @stageName, 
                              project_stage_description = @description, 
                              start_date = @startDate, 
                              end_date = @endDate, 
                              status_id = @statusId, 
                              project_id = @projectId
                          WHERE stage_id = @stageId",
                        connection))
                    {
                        cmd.Parameters.AddWithValue("@stageId", stageId);
                        cmd.Parameters.AddWithValue("@stageName", stageName);
                        cmd.Parameters.AddWithValue("@description", description ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@statusId", statusId);
                        cmd.Parameters.AddWithValue("@projectId", projectId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка при обновлении стадии проекта: {ex.Message}",
                                  "Ошибка базы данных",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                    return false;
                }
            }
        }

        public bool DeleteProjectStage(int stageId)
        {
            int currentEmployeeId = Authorization.currentUser.UserId;

            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    bool isAdmin = IsCurrentUserAdmin(connection);

                    if (!isAdmin)
                    {
                        using (var checkCmd = new NpgsqlCommand(
                            @"SELECT COUNT(*) 
                              FROM task t
                              JOIN team_employee te ON t.team_id = te.team_id
                              WHERE t.stage_id = @stageId AND te.employee_id = @employeeId",
                            connection))
                        {
                            checkCmd.Parameters.AddWithValue("@stageId", stageId);
                            checkCmd.Parameters.AddWithValue("@employeeId", currentEmployeeId);
                            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (count == 0)
                            {
                                MessageBox.Show("У вас нет прав для удаления этого этапа проекта",
                                              "Ошибка доступа",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Warning);
                                return false;
                            }
                        }
                    }

                    using (var cmd = new NpgsqlCommand(
                        "DELETE FROM project_stage WHERE stage_id = @stageId",
                        connection))
                    {
                        cmd.Parameters.AddWithValue("@stageId", stageId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка при удалении стадии проекта: {ex.Message}",
                                  "Ошибка базы данных",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                    return false;
                }
            }
        }
    }
}