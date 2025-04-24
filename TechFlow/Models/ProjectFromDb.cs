using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using TechFlow.Classes;
using TechFlow.Windows;

namespace TechFlow.Models
{
    class ProjectFromDb
    {
        public List<Project> LoadProjects()
        {
            List<Project> projects = new List<Project>();
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
                FROM public.project p
                JOIN public.client c ON p.client_id = c.client_id
                JOIN public.status s ON p.status_id = s.status_id
                WHERE s.status_name != 'На модерации' " +
                        (isAdmin ? "" : @"AND p.project_id IN (
                    SELECT DISTINCT ps.project_id 
                    FROM project_stage ps
                    JOIN task t ON ps.stage_id = t.stage_id
                    JOIN team_employee te ON t.team_id = te.team_id
                    WHERE te.employee_id = @employeeId
                )") +
                        " ORDER BY p.project_id;";

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
                                projects.Add(new Project(
                                    projectId: Convert.ToInt32(reader["project_id"]),
                                    projectName: reader["project_name"].ToString(),
                                    projectDescription: reader["project_description"]?.ToString() ?? "",
                                    startDate: Convert.ToDateTime(reader["start_date"]),
                                    endDate: reader["end_date"] != DBNull.Value ? Convert.ToDateTime(reader["end_date"]) : (DateTime?)null,
                                    clientId: Convert.ToInt32(reader["client_id"]),
                                    projectType: reader["project_type"]?.ToString() ?? "",
                                    budget: reader["budget"] != DBNull.Value ? Convert.ToDecimal(reader["budget"]) : (decimal?)null,
                                    requirements: reader["requirements"]?.ToString() ?? "",
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
                    MessageBox.Show("Ошибка загрузки проектов: " + ex.Message);
                }
            }
            return projects;
        }

        public List<Project> SearchProjects(string searchText)
        {
            List<Project> projects = new List<Project>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
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
                AND s.status_name != 'На модерации'
                ORDER BY p.project_id";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@search", $"%{searchText}%");

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
            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sqlExp = @"
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
                FROM public.project p
                JOIN public.client c ON p.client_id = c.client_id
                JOIN public.status s ON p.status_id = s.status_id
                WHERE p.project_id = @projectId;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.Parameters.AddWithValue("@projectId", projectId);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Project(
                                    projectId: Convert.ToInt32(reader["project_id"]),
                                    projectName: reader["project_name"].ToString(),
                                    projectDescription: reader["project_description"]?.ToString() ?? "",
                                    startDate: Convert.ToDateTime(reader["start_date"]),
                                    endDate: reader["end_date"] != DBNull.Value ? Convert.ToDateTime(reader["end_date"]) : (DateTime?)null,
                                    clientId: Convert.ToInt32(reader["client_id"]),
                                    projectType: reader["project_type"]?.ToString() ?? "",
                                    budget: reader["budget"] != DBNull.Value ? Convert.ToDecimal(reader["budget"]) : (decimal?)null,
                                    requirements: reader["requirements"]?.ToString() ?? "",
                                    isUrgent: Convert.ToBoolean(reader["is_urgent"]),
                                    isConfidential: Convert.ToBoolean(reader["is_confidential"]),
                                    createdAt: Convert.ToDateTime(reader["created_at"]),
                                    clientName: reader["client_name"].ToString(),
                                    status: reader["status"].ToString()
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
            List<Project> projects = new List<Project>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sqlExp = @"
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
                FROM public.project p
                JOIN public.client c ON p.client_id = c.client_id
                JOIN public.status s ON p.status_id = s.status_id
                WHERE s.status_name = 'На модерации'
                ORDER BY p.created_at DESC;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            projects.Add(new Project(
                                projectId: Convert.ToInt32(reader["project_id"]),
                                projectName: reader["project_name"].ToString(),
                                projectDescription: reader["project_description"]?.ToString() ?? "",
                                startDate: Convert.ToDateTime(reader["start_date"]),
                                endDate: reader["end_date"] != DBNull.Value ? Convert.ToDateTime(reader["end_date"]) : (DateTime?)null,
                                clientId: Convert.ToInt32(reader["client_id"]),
                                projectType: reader["project_type"]?.ToString() ?? "",
                                budget: reader["budget"] != DBNull.Value ? Convert.ToDecimal(reader["budget"]) : (decimal?)null,
                                requirements: reader["requirements"]?.ToString() ?? "",
                                isUrgent: Convert.ToBoolean(reader["is_urgent"]),
                                isConfidential: Convert.ToBoolean(reader["is_confidential"]),
                                createdAt: Convert.ToDateTime(reader["created_at"]),
                                clientName: reader["client_name"].ToString(),
                                status: reader["status"].ToString()
                            ));
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки проектов на модерации: " + ex.Message);
                }
            }
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

                    int moderationStatusId;
                    string statusQuery = "SELECT status_id FROM status WHERE status_name = 'На модерации'";

                    using (var statusCmd = new NpgsqlCommand(statusQuery, connection))
                    {
                        var result = statusCmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            MessageBox.Show("Не найден статус 'На модерации' в базе данных");
                            return false;
                        }
                        moderationStatusId = Convert.ToInt32(result);
                    }

                    string sql = @"
                INSERT INTO project (
                    status_id, project_name, project_description, 
                    start_date, end_date, client_id, 
                    project_type, budget, requirements,
                    is_urgent, is_confidential, created_at
                ) 
                VALUES (
                    @statusId, @projectName, @projectDescription,
                    @startDate, @endDate, @clientId,
                    @projectType, @budget, @requirements,
                    @isUrgent, @isConfidential, @createdAt
                )";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@statusId", moderationStatusId);
                        cmd.Parameters.AddWithValue("@projectName", projectName);
                        cmd.Parameters.AddWithValue("@projectDescription", projectDescription);
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@clientId", clientId);
                        cmd.Parameters.AddWithValue("@projectType", projectType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@budget", budget ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@requirements", requirements ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@isUrgent", isUrgent);
                        cmd.Parameters.AddWithValue("@isConfidential", isConfidential);
                        cmd.Parameters.AddWithValue("@createdAt", DateTime.Now);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка при создании проекта: {ex.Message}");
                    return false;
                }
            }
        }

        public bool ModerateProject(int projectId, bool isApproved)
        {
            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sql = @"
                UPDATE project 
                SET is_moderated = TRUE,
                    status_id = (SELECT status_id FROM status WHERE status_name = @statusName)
                WHERE project_id = @projectId";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        string statusName = isApproved ? "В работе" : "Отклонен";
                        cmd.Parameters.AddWithValue("@statusName", statusName);
                        cmd.Parameters.AddWithValue("@projectId", projectId);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка при модерации проекта: {ex.Message}");
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

                    string statusQuery = "SELECT status_id FROM status WHERE status_name = 'Активный'";
                    int activeStatusId;

                    using (var statusCmd = new NpgsqlCommand(statusQuery, connection))
                    {
                        var result = statusCmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            MessageBox.Show("Не найден статус 'Активный' в базе данных");
                            return false;
                        }
                        activeStatusId = Convert.ToInt32(result);
                    }

                    string sql = @"
                UPDATE project 
                SET status_id = @statusId,
                    is_moderated = TRUE
                WHERE project_id = @projectId";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@statusId", activeStatusId);
                        cmd.Parameters.AddWithValue("@projectId", projectId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка при утверждении проекта: {ex.Message}");
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

                    string statusQuery = "SELECT status_id FROM status WHERE status_name = 'Отклонен'";
                    int rejectedStatusId;

                    using (var statusCmd = new NpgsqlCommand(statusQuery, connection))
                    {
                        var result = statusCmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            MessageBox.Show("Не найден статус 'Отклонен' в базе данных");
                            return false;
                        }
                        rejectedStatusId = Convert.ToInt32(result);
                    }

                    string sql = @"
                UPDATE project 
                SET status_id = @statusId,
                    is_moderated = TRUE
                WHERE project_id = @projectId";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@statusId", rejectedStatusId);
                        cmd.Parameters.AddWithValue("@projectId", projectId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка при отклонении проекта: {ex.Message}");
                    return false;
                }
            }
        }

        public List<Project> FilterProjects(string searchText, string status, string clientName,
                                          bool isUrgent, bool isConfidential,
                                          string sortBy = "default")
        {
            List<Project> projects = new List<Project>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    string sql = @"SELECT 
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
                FROM public.project p
                JOIN public.client c ON p.client_id = c.client_id
                JOIN public.status s ON p.status_id = s.status_id
                WHERE s.status_name != 'На модерации'";

                    var conditions = new List<string>();
                    var parameters = new List<NpgsqlParameter>();

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