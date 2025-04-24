using Npgsql;
using System;
using System.Collections.Generic;
using System.Windows;
using TechFlow.Classes;

namespace TechFlow.Models
{
    public class DiscussionFromDb
    {
        public List<Discussion> LoadDiscussions(int taskId)
        {
            List<Discussion> discussions = new List<Discussion>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sqlExp = @"
                SELECT discussion_id, discussion_name, creation_date, completion_date, task_id
                FROM public.discussion
                WHERE task_id = @taskId;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.Parameters.AddWithValue("@taskId", taskId);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                discussions.Add(new Discussion(
                                    reader.GetInt32(0),
                                    reader.GetString(1),
                                    reader.GetDateTime(2),
                                    reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3), 
                                    reader.GetInt32(4) 
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки обсуждений: " + ex.Message);
                }
            }

            return discussions;
        }


        public List<DiscussionComment> LoadComments(int discussionId)
        {
            List<DiscussionComment> comments = new List<DiscussionComment>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sqlExp = @"
                SELECT c.comment_id, c.comment_text, c.creation_date, c.discussion_id, c.employee_id,
                       e.first_name, e.last_name, e.image_path
                FROM public.discussion_comment c
                JOIN public.employee e ON c.employee_id = e.employee_id
                WHERE c.discussion_id = @discussionId
                ORDER BY c.creation_date;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.Parameters.AddWithValue("@discussionId", discussionId);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                comments.Add(new DiscussionComment(
                                    Convert.ToInt32(reader["comment_id"]),
                                    reader["comment_text"].ToString(),
                                    Convert.ToDateTime(reader["creation_date"]),
                                    Convert.ToInt32(reader["discussion_id"]),
                                    Convert.ToInt32(reader["employee_id"]),
                                    reader["first_name"].ToString(),
                                    reader["last_name"].ToString(),
                                    reader["image_path"].ToString()
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки комментариев: " + ex.Message);
                }
            }

            return comments;
        }

        public void AddComment(string commentText, int discussionId, int employeeId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sqlExp = @"
                INSERT INTO public.discussion_comment 
                (comment_text, creation_date, discussion_id, employee_id)
                VALUES (@commentText, @creationDate, @discussionId, @employeeId);";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.Parameters.AddWithValue("@commentText", commentText);
                        command.Parameters.AddWithValue("@creationDate", DateTime.Now);
                        command.Parameters.AddWithValue("@discussionId", discussionId);
                        command.Parameters.AddWithValue("@employeeId", employeeId);

                        command.ExecuteNonQuery();
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка добавления комментария: " + ex.Message);
                }
            }
        }

        public User GetEmployeeById(int employeeId)
        {
            User employee = null;

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    string sqlExp = @"
                SELECT employee_id, first_name, last_name, image_path
                FROM public.employee
                WHERE employee_id = @employeeId;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.Parameters.AddWithValue("@employeeId", employeeId);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                employee = new User
                                {
                                    UserId = Convert.ToInt32(reader["employee_id"]),
                                    FirstName = reader["first_name"].ToString(),
                                    LastName = reader["last_name"].ToString(),
                                    ImagePath = reader["image_path"].ToString()
                                };
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка получения сотрудника: " + ex.Message);
                }
            }

            return employee;
        }


    }
}
