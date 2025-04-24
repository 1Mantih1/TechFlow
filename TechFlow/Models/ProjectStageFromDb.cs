using Npgsql;
using System;
using System.Collections.Generic;
using System.Windows;
using TechFlow.Classes;
using TechFlow.Windows;

namespace TechFlow.Models
{
    class ProjectStageFromDb
    {
        public List<ProjectStage> LoadProjectStages()
        {
            List<ProjectStage> projectStages = new List<ProjectStage>();
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
                SELECT ps.stage_id, ps.stage_name, ps.project_stage_description, ps.start_date, ps.end_date, 
                       ps.status_id, ps.project_id, s.status_name, p.project_name
                FROM public.project_stage ps
                JOIN public.status s ON ps.status_id = s.status_id
                JOIN public.project p ON ps.project_id = p.project_id " +
                        (isAdmin ? "" : @"WHERE ps.stage_id IN (
                    SELECT DISTINCT t.stage_id 
                    FROM task t
                    JOIN team_employee te ON t.team_id = te.team_id
                    WHERE te.employee_id = @employeeId
                )") +
                        " ORDER BY ps.stage_id;";

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
                    MessageBox.Show("Ошибка загрузки стадий проекта: " + ex.Message);
                }
            }
            return projectStages;
        }

        public List<ProjectStage> SearchProjectStages(string searchText)
        {
            List<ProjectStage> projectStages = new List<ProjectStage>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sql = @"
                SELECT ps.stage_id, ps.stage_name, ps.project_stage_description, ps.start_date, ps.end_date, 
                       ps.status_id, ps.project_id, s.status_name, p.project_name
                FROM public.project_stage ps
                JOIN public.status s ON ps.status_id = s.status_id
                JOIN public.project p ON ps.project_id = p.project_id
                WHERE ps.stage_name ILIKE @search
                ORDER BY ps.stage_id;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@search", $"%{searchText}%");

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

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    string sql = @"
                SELECT ps.stage_id, ps.stage_name, ps.project_stage_description, ps.start_date, ps.end_date, 
                       ps.status_id, ps.project_id, s.status_name, p.project_name
                FROM public.project_stage ps
                JOIN public.status s ON ps.status_id = s.status_id
                JOIN public.project p ON ps.project_id = p.project_id";

                    var conditions = new List<string>();
                    var parameters = new List<NpgsqlParameter>();

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        conditions.Add("ps.stage_name ILIKE @search");
                        parameters.Add(new NpgsqlParameter("@search", $"%{searchText}%"));
                    }

                    if (!string.IsNullOrEmpty(projectName))
                    {
                        conditions.Add("p.project_name ILIKE @projectName");
                        parameters.Add(new NpgsqlParameter("@projectName", $"%{projectName}%"));
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        conditions.Add("s.status_name = @status");
                        parameters.Add(new NpgsqlParameter("@status", status));
                    }

                    if (!string.IsNullOrWhiteSpace(dateFilterOption))
                    {
                        switch (dateFilterOption)
                        {
                            case "Сегодня":
                                conditions.Add("ps.start_date::date = CURRENT_DATE");
                                break;
                            case "На этой неделе":
                                conditions.Add("ps.start_date >= date_trunc('week', CURRENT_DATE)");
                                break;
                            case "В этом месяце":
                                conditions.Add("ps.start_date >= date_trunc('month', CURRENT_DATE)");
                                break;
                            case "Просроченные":
                                conditions.Add("ps.end_date IS NOT NULL AND ps.end_date < CURRENT_DATE");
                                break;
                            default:
                                break;
                        }
                    }

                    if (isUrgent)
                    {
                        conditions.Add("ps.end_date IS NOT NULL AND ps.end_date <= CURRENT_DATE + INTERVAL '3 days'");
                    }

                    if (conditions.Count > 0)
                    {
                        sql += " WHERE " + string.Join(" AND ", conditions);
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
    }
}
