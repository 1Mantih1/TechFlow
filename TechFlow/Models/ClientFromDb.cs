using Npgsql;
using System.Collections.Generic;
using System.Windows;

namespace TechFlow.Models
{
    public class ClientFromDb
    {
        public List<ClientFromDb> LoadClients()
        {
            List<ClientFromDb> clients = new List<ClientFromDb>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sqlExp = @"SELECT client_id, organization_name, first_name, last_name, email, phone, login FROM public.client ORDER BY client_id;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clients.Add(new ClientFromDb());
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки клиентов: " + ex.Message);
                }
            }
            return clients;
        }
    }
}
