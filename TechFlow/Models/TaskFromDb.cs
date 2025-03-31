using Npgsql;
using System.Collections.Generic;
using System.Windows;
using System;
using TechFlow.Classes;

namespace TechFlow.Models
{
    class TaskFromDb
    {
        public List<ProjectTask> LoadTasks()
        {
            List<ProjectTask> tasks = new List<ProjectTask>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sqlExp = @"
                        SELECT t.task_id, t.task_name, t.task_description, t.start_date, t.end_date, 
                               t.status_id, t.stage_id, t.team_id, 
                               s.status_name, ps.stage_name, tm.team_name
                        FROM public.task t
                        INNER JOIN public.status s ON t.status_id = s.status_id
                        INNER JOIN public.project_stage ps ON t.stage_id = ps.stage_id
                        INNER JOIN public.team tm ON t.team_id = tm.team_id
                        ORDER BY t.task_id;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
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
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки задач: " + ex.Message);
                }
            }
            return tasks;
        }
    }
}
