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
    class ProjectFromDb
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

        public List<Project> LoadProjects()
        {
            List<Project> projects = new List<Project>();
            int currentEmployeeId = Authorization.currentUser.UserId;

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    bool isAdmin = IsCurrentUserAdmin(connection);

                    string sql = @"
                        SELECT 
                            p.project_id, 
                            p.project_name, 
                            p.project_description,
                            p.start_date, 
                            p.end_date, 
                            p.client_id, 
                            p.project_type,
                            p.budget, 
                            p.requirements, 
                            p.is_urgent, 
                            p.is_confidential,
                            p.created_at,
                            c.organization_name AS client_name,
                            s.status_name AS status
                        FROM project p
                        JOIN client c ON p.client_id = c.client_id
                        JOIN status s ON p.status_id = s.status_id
                        WHERE s.status_name != 'На модерации'";

                    if (!isAdmin)
                    {
                        sql += @"
                        AND EXISTS (
                            SELECT 1 FROM project_stage ps
                            JOIN task t ON t.stage_id = ps.stage_id
                            JOIN team_employee te ON t.team_id = te.team_id
                            WHERE ps.project_id = p.project_id AND te.employee_id = @employeeId
                        )";
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
                                projects.Add(new Project(
                                    projectId: reader.GetInt32(reader.GetOrdinal("project_id")),
                                    projectName: reader.GetString(reader.GetOrdinal("project_name")),
                                    projectDescription: reader.IsDBNull(reader.GetOrdinal("project_description")) ? "" : reader.GetString(reader.GetOrdinal("project_description")),
                                    startDate: reader.GetDateTime(reader.GetOrdinal("start_date")),
                                    endDate: reader.IsDBNull(reader.GetOrdinal("end_date")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("end_date")),
                                    clientId: reader.GetInt32(reader.GetOrdinal("client_id")),
                                    projectType: reader.IsDBNull(reader.GetOrdinal("project_type")) ? "" : reader.GetString(reader.GetOrdinal("project_type")),
                                    budget: reader.IsDBNull(reader.GetOrdinal("budget")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("budget")),
                                    requirements: reader.IsDBNull(reader.GetOrdinal("requirements")) ? "" : reader.GetString(reader.GetOrdinal("requirements")),
                                    isUrgent: reader.GetBoolean(reader.GetOrdinal("is_urgent")),
                                    isConfidential: reader.GetBoolean(reader.GetOrdinal("is_confidential")),
                                    createdAt: reader.GetDateTime(reader.GetOrdinal("created_at")),
                                    clientName: reader.GetString(reader.GetOrdinal("client_name")),
                                    status: reader.GetString(reader.GetOrdinal("status"))
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки проектов: " + ex.Message);
                }
            }

            return projects;
        }

        public List<Project> SearchProjects(string searchText)
        {
            List<Project> projects = new List<Project>();
            int currentEmployeeId = Authorization.currentUser.UserId;

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    bool isAdmin = IsCurrentUserAdmin(connection);

                    string sql = @"
                        SELECT 
                            p.project_id, 
                            p.project_name, 
                            p.project_description,
                            p.start_date, 
                            p.end_date, 
                            p.client_id, 
                            p.project_type,
                            p.budget, 
                            p.requirements, 
                            p.is_urgent, 
                            p.is_confidential,
                            p.created_at,
                            c.organization_name AS client_name,
                            s.status_name AS status
                        FROM project p
                        JOIN client c ON p.client_id = c.client_id
                        JOIN status s ON p.status_id = s.status_id
                        WHERE p.project_name ILIKE @search
                        AND s.status_name != 'На модерации'";

                    if (!isAdmin)
                    {
                        sql += @"
                        AND EXISTS (
                            SELECT 1 FROM project_stage ps
                            JOIN task t ON t.stage_id = ps.stage_id
                            JOIN team_employee te ON t.team_id = te.team_id
                            WHERE ps.project_id = p.project_id AND te.employee_id = @employeeId
                        )";
                    }

                    sql += " ORDER BY p.project_id";

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
                                projects.Add(new Project(
                                    projectId: Convert.ToInt32(reader["project_id"]),
                                    projectName: reader["project_name"].ToString(),
                                    projectDescription: reader["project_description"] != DBNull.Value
                                        ? reader["project_description"].ToString()
                                        : string.Empty,
                                    startDate: Convert.ToDateTime(reader["start_date"]),
                                    endDate: reader["end_date"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["end_date"])
                                        : (DateTime?)null,
                                    clientId: Convert.ToInt32(reader["client_id"]),
                                    projectType: reader["project_type"] != DBNull.Value
                                        ? reader["project_type"].ToString()
                                        : string.Empty,
                                    budget: reader["budget"] != DBNull.Value
                                        ? Convert.ToDecimal(reader["budget"])
                                        : (decimal?)null,
                                    requirements: reader["requirements"] != DBNull.Value
                                        ? reader["requirements"].ToString()
                                        : string.Empty,
                                    isUrgent: Convert.ToBoolean(reader["is_urgent"]),
                                    isConfidential: Convert.ToBoolean(reader["is_confidential"]),
                                    createdAt: Convert.ToDateTime(reader["created_at"]),
                                    clientName: reader["client_name"].ToString(),
                                    status: reader["status"].ToString()
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка при поиске проектов: {ex.Message}",
                                  "Ошибка базы данных",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
            return projects;
        }

        public Project GetProjectById(int projectId)
        {
            int currentEmployeeId = Authorization.currentUser.UserId;

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    bool isAdmin = IsCurrentUserAdmin(connection);

                    string sql = @"
                        SELECT 
                            p.project_id, 
                            p.project_name, 
                            p.project_description,
                            p.start_date, 
                            p.end_date, 
                            p.client_id, 
                            p.project_type,
                            p.budget, 
                            p.requirements, 
                            p.is_urgent, 
                            p.is_confidential,
                            p.created_at,
                            c.organization_name AS client_name,
                            s.status_name AS status
                        FROM project p
                        JOIN client c ON p.client_id = c.client_id
                        JOIN status s ON p.status_id = s.status_id
                        WHERE p.project_id = @projectId";

                    if (!isAdmin)
                    {
                        sql += @"
                        AND EXISTS (
                            SELECT 1 FROM project_stage ps
                            JOIN task t ON t.stage_id = ps.stage_id
                            JOIN team_employee te ON t.team_id = te.team_id
                            WHERE ps.project_id = p.project_id AND te.employee_id = @employeeId
                        )";
                    }

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@projectId", projectId);

                        if (!isAdmin)
                        {
                            command.Parameters.AddWithValue("@employeeId", currentEmployeeId);
                        }

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Project(
                                    projectId: reader.GetInt32(reader.GetOrdinal("project_id")),
                                    projectName: reader.GetString(reader.GetOrdinal("project_name")),
                                    projectDescription: reader.IsDBNull(reader.GetOrdinal("project_description")) ? "" : reader.GetString(reader.GetOrdinal("project_description")),
                                    startDate: reader.GetDateTime(reader.GetOrdinal("start_date")),
                                    endDate: reader.IsDBNull(reader.GetOrdinal("end_date")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("end_date")),
                                    clientId: reader.GetInt32(reader.GetOrdinal("client_id")),
                                    projectType: reader.IsDBNull(reader.GetOrdinal("project_type")) ? "" : reader.GetString(reader.GetOrdinal("project_type")),
                                    budget: reader.IsDBNull(reader.GetOrdinal("budget")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("budget")),
                                    requirements: reader.IsDBNull(reader.GetOrdinal("requirements")) ? "" : reader.GetString(reader.GetOrdinal("requirements")),
                                    isUrgent: reader.GetBoolean(reader.GetOrdinal("is_urgent")),
                                    isConfidential: reader.GetBoolean(reader.GetOrdinal("is_confidential")),
                                    createdAt: reader.GetDateTime(reader.GetOrdinal("created_at")),
                                    clientName: reader.GetString(reader.GetOrdinal("client_name")),
                                    status: reader.GetString(reader.GetOrdinal("status"))
                                );
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка при загрузке проекта: {ex.Message}");
                    return null;
                }
            }
            return null;
        }

        public List<Project> LoadModeratingProjects()
        {
            var projects = new List<Project>();

            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    // Загружаем проекты, ожидающие модерации (is_moderated = false)
                    string sql = @"
                SELECT 
                    p.project_id, p.project_name, p.project_description,
                    p.start_date, p.end_date, p.client_id, p.project_type,
                    p.budget, p.requirements, p.is_urgent, p.is_confidential,
                    p.created_at, c.organization_name, s.status_name
                FROM project p
                JOIN client c ON p.client_id = c.client_id
                JOIN status s ON p.status_id = s.status_id
                WHERE p.is_moderated = false
                ORDER BY p.created_at DESC";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                projects.Add(new Project(
                                    projectId: reader.GetInt32(0),
                                    projectName: reader.GetString(1),
                                    projectDescription: reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    startDate: reader.GetDateTime(3),
                                    endDate: reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                    clientId: reader.GetInt32(5),
                                    projectType: reader.IsDBNull(6) ? "" : reader.GetString(6),
                                    budget: reader.IsDBNull(7) ? (decimal?)null : reader.GetDecimal(7),
                                    requirements: reader.IsDBNull(8) ? "" : reader.GetString(8),
                                    isUrgent: reader.GetBoolean(9),
                                    isConfidential: reader.GetBoolean(10),
                                    createdAt: reader.GetDateTime(11),
                                    clientName: reader.GetString(12),
                                    status: reader.GetString(13)
                                ));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки проектов: {ex.Message}");
                    // Можно добавить MessageBox.Show для пользователя
                }
            }

            Console.WriteLine($"Загружено проектов на модерацию: {projects.Count}");
            return projects;
        }

        public bool CreateProject(
            string projectName,
            string projectDescription,
            DateTime startDate,
            DateTime? endDate,
            int clientId,
            string projectType = null,
            decimal? budget = null,
            string requirements = null,
            bool isUrgent = false,
            bool isConfidential = false)
        {
            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    // 1. Получаем ID статуса "На модерации"
                    int moderationStatusId;
                    using (var cmd = new NpgsqlCommand("SELECT status_id FROM status WHERE status_name = 'На модерации'", connection))
                    {
                        var result = cmd.ExecuteScalar();
                        if (result == null || result is DBNull)
                        {
                            MessageBox.Show("Статус 'На модерации' не найден в базе данных", "Ошибка");
                            return false;
                        }
                        moderationStatusId = Convert.ToInt32(result);
                    }

                    // 2. Вызываем хранимую процедуру
                    using (var cmd = new NpgsqlCommand("sp_create_project", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Добавляем параметры
                        cmd.Parameters.Add(new NpgsqlParameter("p_project_name", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = projectName });
                        cmd.Parameters.Add(new NpgsqlParameter("p_project_description", NpgsqlTypes.NpgsqlDbType.Text) { Value = (object)projectDescription ?? DBNull.Value });
                        cmd.Parameters.Add(new NpgsqlParameter("p_start_date", NpgsqlTypes.NpgsqlDbType.Date) { Value = startDate });
                        cmd.Parameters.Add(new NpgsqlParameter("p_end_date", NpgsqlTypes.NpgsqlDbType.Date) { Value = (object)endDate ?? DBNull.Value });
                        cmd.Parameters.Add(new NpgsqlParameter("p_client_id", NpgsqlTypes.NpgsqlDbType.Integer) { Value = clientId });
                        cmd.Parameters.Add(new NpgsqlParameter("p_status_id", NpgsqlTypes.NpgsqlDbType.Integer) { Value = moderationStatusId });
                        cmd.Parameters.Add(new NpgsqlParameter("p_project_type", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = (object)projectType ?? DBNull.Value });
                        cmd.Parameters.Add(new NpgsqlParameter("p_budget", NpgsqlTypes.NpgsqlDbType.Numeric) { Value = (object)budget ?? DBNull.Value });
                        cmd.Parameters.Add(new NpgsqlParameter("p_requirements", NpgsqlTypes.NpgsqlDbType.Text) { Value = (object)requirements ?? DBNull.Value });
                        cmd.Parameters.Add(new NpgsqlParameter("p_is_urgent", NpgsqlTypes.NpgsqlDbType.Boolean) { Value = isUrgent });
                        cmd.Parameters.Add(new NpgsqlParameter("p_is_confidential", NpgsqlTypes.NpgsqlDbType.Boolean) { Value = isConfidential });

                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (PostgresException pgEx)
                {
                    string errorMessage;
                    if (pgEx.SqlState == "23503")
                        errorMessage = "Ошибка внешнего ключа. Проверьте существование клиента и статуса.";
                    else if (pgEx.SqlState == "23505")
                        errorMessage = "Проект с таким именем уже существует.";
                    else
                        errorMessage = $"Ошибка базы данных: {pgEx.Message}";

                    MessageBox.Show(errorMessage, "Ошибка создания проекта");
                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании проекта: {ex.Message}", "Ошибка");
                    return false;
                }
            }
        }

        public bool ApproveProject(int projectId)
        {
            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    string sql = @"
                UPDATE project 
                SET is_moderated = true,
                    status_id = (SELECT status_id FROM status WHERE status_name = 'Активный')
                WHERE project_id = @projectId";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@projectId", projectId);
                        int affected = cmd.ExecuteNonQuery();
                        return affected > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка утверждения проекта: {ex.Message}");
                    return false;
                }
            }
        }

        public bool RejectProject(int projectId)
        {
            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    string sql = @"
                UPDATE project 
                SET is_moderated = true,
                    status_id = (SELECT status_id FROM status WHERE status_name = 'Активный')
                WHERE project_id = @projectId";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@projectId", projectId);
                        int affected = cmd.ExecuteNonQuery();
                        return affected > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка отклонения проекта: {ex.Message}");
                    return false;
                }
            }
        }

        public List<Project> FilterProjects(
            string searchText,
            string status,
            string clientName,
            bool isUrgent,
            bool isConfidential,
            string sortBy = "default")
        {
            List<Project> projects = new List<Project>();
            int currentEmployeeId = Authorization.currentUser.UserId;

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    bool isAdmin = IsCurrentUserAdmin(connection);

                    string sql = @"
                        SELECT 
                            p.project_id, 
                            p.project_name, 
                            p.project_description, 
                            p.start_date, 
                            p.end_date, 
                            p.client_id, 
                            p.project_type, 
                            p.budget, 
                            p.requirements, 
                            p.is_urgent, 
                            p.is_confidential, 
                            p.created_at,
                            c.organization_name AS client_name,
                            s.status_name AS status
                        FROM project p
                        JOIN client c ON p.client_id = c.client_id
                        JOIN status s ON p.status_id = s.status_id
                        WHERE s.status_name != 'На модерации'";

                    if (!isAdmin)
                    {
                        sql += @"
                        AND EXISTS (
                            SELECT 1 FROM project_stage ps
                            JOIN task t ON t.stage_id = ps.stage_id
                            JOIN team_employee te ON t.team_id = te.team_id
                            WHERE ps.project_id = p.project_id AND te.employee_id = @employeeId
                        )";
                    }

                    var conditions = new List<string>();
                    var parameters = new List<NpgsqlParameter>();

                    if (!isAdmin)
                    {
                        parameters.Add(new NpgsqlParameter("@employeeId", currentEmployeeId));
                    }

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        conditions.Add("(p.project_name ILIKE @search OR c.organization_name ILIKE @search)");
                        parameters.Add(new NpgsqlParameter("@search", $"%{searchText}%"));
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        conditions.Add("s.status_name = @status");
                        parameters.Add(new NpgsqlParameter("@status", status));
                    }

                    if (!string.IsNullOrEmpty(clientName))
                    {
                        conditions.Add("c.organization_name ILIKE @clientName");
                        parameters.Add(new NpgsqlParameter("@clientName", $"%{clientName}%"));
                    }

                    if (isUrgent)
                    {
                        conditions.Add("p.is_urgent = true");
                    }

                    if (isConfidential)
                    {
                        conditions.Add("p.is_confidential = true");
                    }

                    if (conditions.Count > 0)
                    {
                        sql += " AND " + string.Join(" AND ", conditions);
                    }

                    switch (sortBy)
                    {
                        case "endingSoon":
                            sql += " ORDER BY CASE WHEN p.end_date IS NULL THEN 1 ELSE 0 END, p.end_date ASC";
                            break;
                        case "newest":
                            sql += " ORDER BY p.created_at DESC";
                            break;
                        case "urgent":
                            sql += " ORDER BY p.is_urgent DESC, p.end_date ASC";
                            break;
                        default:
                            sql += " ORDER BY p.project_id";
                            break;
                    }

                    Debug.WriteLine("Выполняемый SQL: " + sql);
                    foreach (var param in parameters)
                    {
                        Debug.WriteLine($"Параметр: {param.ParameterName} = {param.Value}");
                    }

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                projects.Add(new Project(
                                    Convert.ToInt32(reader["project_id"]),
                                    reader["project_name"].ToString(),
                                    reader["project_description"]?.ToString() ?? "",
                                    Convert.ToDateTime(reader["start_date"]),
                                    reader["end_date"] != DBNull.Value ? Convert.ToDateTime(reader["end_date"]) : (DateTime?)null,
                                    Convert.ToInt32(reader["client_id"]),
                                    reader["project_type"]?.ToString() ?? "",
                                    reader["budget"] != DBNull.Value ? Convert.ToDecimal(reader["budget"]) : (decimal?)null,
                                    reader["requirements"]?.ToString() ?? "",
                                    Convert.ToBoolean(reader["is_urgent"]),
                                    Convert.ToBoolean(reader["is_confidential"]),
                                    Convert.ToDateTime(reader["created_at"]),
                                    reader["client_name"].ToString(),
                                    reader["status"].ToString()
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка фильтрации проектов: " + ex.Message);
                }
            }
            return projects;
        }
    }
}