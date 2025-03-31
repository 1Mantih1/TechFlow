using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TechFlow.Classes;

namespace TechFlow.Models
{
    class ProjectFromDb
    {
        public List<Project> LoadProjects()
        {
            List<Project> projects = new List<Project>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sqlExp = @"
                        SELECT p.project_id, p.project_name, p.project_description, p.start_date, p.end_date, 
                               c.client_id, c.organization_name, s.status_name 
                        FROM public.project p
                        INNER JOIN public.client c ON p.client_id = c.client_id
                        INNER JOIN public.status s ON p.status_id = s.status_id
                        ORDER BY p.project_id;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
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
                                reader["organization_name"].ToString(),
                                reader["status_name"].ToString()
                            ));
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
    }
}
