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
    class TeamEmployeeFromDb
    {
        public List<TeamEmployee> LoadTeamEmployees()
        {
            List<TeamEmployee> teamEmployees = new List<TeamEmployee>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sqlExp = @"
                        SELECT te.team_employee_id, te.employee_role_id, te.team_id, te.employee_id, 
                               er.employee_role_name, t.team_name, 
                               CONCAT(e.first_name, ' ', e.last_name) AS employee_name,
                               e.image_path AS employee_image
                        FROM public.team_employee te
                        INNER JOIN public.employee_role er ON te.employee_role_id = er.employee_role_id
                        INNER JOIN public.team t ON te.team_id = t.team_id
                        INNER JOIN public.employee e ON te.employee_id = e.employee_id
                        ORDER BY te.team_employee_id;
                    ";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            teamEmployees.Add(new TeamEmployee(
                                Convert.ToInt32(reader["team_employee_id"]),
                                Convert.ToInt32(reader["employee_role_id"]),
                                Convert.ToInt32(reader["team_id"]),
                                Convert.ToInt32(reader["employee_id"]),
                                reader["employee_role_name"].ToString(),
                                reader["team_name"].ToString(),
                                reader["employee_name"].ToString(),
                                reader["employee_image"].ToString()
                            ));
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки данных о сотрудниках в командах: " + ex.Message);
                }
            }
            return teamEmployees;
        }
    }
}
