using Npgsql;
using System.Collections.Generic;
using System.Windows;
using TechFlow.Classes;

namespace TechFlow.Models
{
    public class ClientFromDb
    {
        public List<Client> LoadClients()
        {
            List<Client> clients = new List<Client>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sqlExp = "SELECT * FROM clients_view;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clients.Add(new Client(
                                clientId: reader.GetInt32(reader.GetOrdinal("client_id")),
                                organizationName: reader.GetString(reader.GetOrdinal("organization_name")),
                                firstName: reader.GetString(reader.GetOrdinal("first_name")),
                                lastName: reader.GetString(reader.GetOrdinal("last_name")),
                                email: reader.GetString(reader.GetOrdinal("email")),
                                phone: reader.GetString(reader.GetOrdinal("phone")),
                                login: reader.GetString(reader.GetOrdinal("login"))
                            ));
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