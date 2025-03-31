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
    class TeamFromDb
    {
        public List<Team> LoadTeams()
        {
            List<Team> teams = new List<Team>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sqlExp = @"
                        SELECT t.team_id, t.team_name, t.team_description, t.organization_date, t.completion_date 
                        FROM public.team t
                        ORDER BY t.team_id;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
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
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки команд: " + ex.Message);
                }
            }
            return teams;
        }
    }
}
