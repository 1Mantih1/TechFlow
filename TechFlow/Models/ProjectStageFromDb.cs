using Npgsql;
using System;
using System.Collections.Generic;
using System.Windows;
using TechFlow.Classes;

namespace TechFlow.Models
{
    class ProjectStageFromDb
    {
        public List<ProjectStage> LoadProjectStages()
        {
            List<ProjectStage> projectStages = new List<ProjectStage>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sqlExp = @"
                SELECT ps.stage_id, ps.stage_name, ps.project_stage_description, ps.start_date, ps.end_date, 
                       ps.status_id, ps.project_id, s.status_name, p.project_name
                FROM public.project_stage ps
                JOIN public.status s ON ps.status_id = s.status_id
                JOIN public.project p ON ps.project_id = p.project_id
                ORDER BY ps.stage_id;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
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
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки стадий проекта: " + ex.Message);
                }
            }
            return projectStages;
        }

    }
}
