using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TechFlow.Classes;
using TechFlow.Windows;

namespace TechFlow.Models
{
    class TeamFromDb
    {
        public List<Team> LoadTeams()
        {
            List<Team> teams = new List<Team>();
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
                SELECT t.team_id, t.team_name, t.team_description, t.organization_date, t.completion_date 
                FROM public.team t " +
                        (isAdmin ? "" : "WHERE t.team_id IN (" +
                            "SELECT te.team_id FROM team_employee te " +
                            "WHERE te.employee_id = @employeeId)") +
                        " ORDER BY t.team_id;";

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
                                teams.Add(new Team(
                                    Convert.ToInt32(reader["team_id"]),
                                    reader["team_name"].ToString(),
                                    reader["team_description"]?.ToString() ?? "",
                                    Convert.ToDateTime(reader["organization_date"]),
                                    reader["completion_date"] != DBNull.Value ? Convert.ToDateTime(reader["completion_date"]) : (DateTime?)null
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки команд: " + ex.Message);
                }
            }
            return teams;
        }

        public List<Team> SearchTeams(string searchText)
        {
            List<Team> teams = new List<Team>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sql = @"
                SELECT t.team_id, t.team_name, t.team_description, 
                       t.organization_date, t.completion_date 
                FROM public.team t
                WHERE t.team_name ILIKE @search
                ORDER BY t.team_id";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@search", $"%{searchText}%");

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                teams.Add(new Team(
                                    Convert.ToInt32(reader["team_id"]),
                                    reader["team_name"].ToString(),
                                    reader["team_description"]?.ToString() ?? "",
                                    Convert.ToDateTime(reader["organization_date"]),
                                    reader["completion_date"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["completion_date"])
                                        : (DateTime?)null
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка поиска команд: {ex.Message}",
                                  "Ошибка базы данных",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
            return teams;
        }

        public List<Team> FilterTeams(
            string searchText,
            string taskSearchText,
            string dateFilterOption,
            bool activeOnly,
            string sortBy = "default")
        {
            List<Team> teams = new List<Team>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    string sql = @"
                SELECT DISTINCT t.team_id, t.team_name, t.team_description, 
                       t.organization_date, t.completion_date 
                FROM public.team t
                LEFT JOIN public.task tk ON t.team_id = tk.team_id";

                    var conditions = new List<string>();
                    var parameters = new List<NpgsqlParameter>();

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        conditions.Add("t.team_name ILIKE @search");
                        parameters.Add(new NpgsqlParameter("@search", $"%{searchText}%"));
                    }

                    if (!string.IsNullOrEmpty(taskSearchText))
                    {
                        conditions.Add("tk.task_name ILIKE @taskSearch");
                        parameters.Add(new NpgsqlParameter("@taskSearch", $"%{taskSearchText}%"));
                    }

                    if (!string.IsNullOrWhiteSpace(dateFilterOption) && dateFilterOption != "Любая дата")
                    {
                        switch (dateFilterOption)
                        {
                            case "Сегодня":
                                conditions.Add("t.organization_date::date = CURRENT_DATE");
                                break;
                            case "На этой неделе":
                                conditions.Add("t.organization_date >= date_trunc('week', CURRENT_DATE)");
                                break;
                            case "В этом месяце":
                                conditions.Add("t.organization_date >= date_trunc('month', CURRENT_DATE)");
                                break;
                            case "С завершенными задачами":
                                conditions.Add("EXISTS (SELECT 1 FROM task WHERE team_id = t.team_id AND end_date IS NOT NULL)");
                                break;
                        }
                    }

                    if (activeOnly)
                    {
                        conditions.Add("t.completion_date IS NULL");
                    }

                    if (conditions.Count > 0)
                    {
                        sql += " WHERE " + string.Join(" AND ", conditions);
                    }

                    switch (sortBy)
                    {
                        case "newest":
                            sql += " ORDER BY t.organization_date DESC";
                            break;
                        case "oldest":
                            sql += " ORDER BY t.organization_date ASC";
                            break;
                        case "mostTasks":
                            sql += " ORDER BY (SELECT COUNT(*) FROM task WHERE team_id = t.team_id) DESC";
                            break;
                        default:
                            sql += " ORDER BY t.team_id";
                            break;
                    }

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                teams.Add(new Team(
                                    Convert.ToInt32(reader["team_id"]),
                                    reader["team_name"].ToString(),
                                    reader["team_description"]?.ToString() ?? "",
                                    Convert.ToDateTime(reader["organization_date"]),
                                    reader["completion_date"] != DBNull.Value ? Convert.ToDateTime(reader["completion_date"]) : (DateTime?)null
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка фильтрации команд: " + ex.Message);
                }
            }

            return teams;
        }

        public int AddTeam(Team team)
        {
            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                connection.Open();

                const string query = @"
        INSERT INTO team (team_name, team_description, organization_date, completion_date)
        VALUES (@TeamName, @TeamDescription, @OrganizationDate, @CompletionDate)
        RETURNING team_id;";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TeamName", team.TeamName);
                    command.Parameters.AddWithValue("@TeamDescription", team.TeamDescription != null ? (object)team.TeamDescription : DBNull.Value);
                    command.Parameters.AddWithValue("@OrganizationDate", team.OrganizationDate);
                    command.Parameters.AddWithValue("@CompletionDate", team.CompletionDate.HasValue ? (object)team.CompletionDate.Value : DBNull.Value);

                    return (int)command.ExecuteScalar();
                }
            }
        }


        public bool UpdateTeam(Team team)
        {
            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                connection.Open();

                const string query = @"
        UPDATE team 
        SET 
            team_name = @TeamName,
            team_description = @TeamDescription,
            organization_date = @OrganizationDate,
            completion_date = @CompletionDate
        WHERE team_id = @TeamId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TeamId", team.TeamId);
                    command.Parameters.AddWithValue("@TeamName", team.TeamName);
                    command.Parameters.AddWithValue("@TeamDescription", team.TeamDescription != null ? (object)team.TeamDescription : DBNull.Value);
                    command.Parameters.AddWithValue("@OrganizationDate", team.OrganizationDate);
                    command.Parameters.AddWithValue("@CompletionDate", team.CompletionDate.HasValue ? (object)team.CompletionDate.Value : DBNull.Value);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
