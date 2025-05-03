using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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
                SELECT 
                    d.discussion_id,
                    d.discussion_name,
                    d.creation_date,
                    d.completion_date,
                    d.task_id
                FROM discussion d
                WHERE d.task_id = @taskId
                ORDER BY d.creation_date DESC";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.Parameters.AddWithValue("@taskId", taskId);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    discussions.Add(new Discussion(
                                        reader.GetInt32(reader.GetOrdinal("discussion_id")),
                                        reader.GetString(reader.GetOrdinal("discussion_name")),
                                        reader.GetDateTime(reader.GetOrdinal("creation_date")),
                                        reader.IsDBNull(reader.GetOrdinal("completion_date")) ?
                                            (DateTime?)null :
                                            reader.GetDateTime(reader.GetOrdinal("completion_date")),
                                        reader.GetInt32(reader.GetOrdinal("task_id"))
                                    ));
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Ошибка при чтении обсуждения: {ex.Message}");
                                }
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка загрузки обсуждений: {ex.Message}");
                }
            }

            return discussions;
        }

        public int GetDiscussionIdByTaskId(int taskId)
        {
            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                connection.Open();
                var cmd = new NpgsqlCommand(
                    "SELECT discussion_id FROM discussion WHERE task_id = @taskId",
                    connection);
                cmd.Parameters.AddWithValue("@taskId", taskId);

                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        // Обновлённый метод загрузки комментариев
        public List<DiscussionComment> LoadComments(int discussionId)
        {
            var comments = new List<DiscussionComment>();

            if (discussionId <= 0) return comments;

            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    var sql = @"SELECT 
                            dc.comment_id,
                            dc.comment_text,
                            dc.creation_date,
                            dc.discussion_id,
                            dc.employee_id,
                            e.first_name,
                            e.last_name,
                            e.image_path
                        FROM discussion_comment dc
                        JOIN employee e ON dc.employee_id = e.employee_id
                        WHERE dc.discussion_id = @disc_id
                        ORDER BY dc.creation_date";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@disc_id", discussionId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                comments.Add(new DiscussionComment(
                                    reader.GetInt32(0),
                                    reader.GetString(1),
                                    reader.GetDateTime(2),
                                    reader.GetInt32(3),
                                    reader.GetInt32(4),
                                    reader.GetString(5),
                                    reader.GetString(6),
                                    reader.GetString(7)
                                ));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки комментариев: {ex.Message}");
                }
            }

            return comments;
        }

        public bool AddComment(string commentText, int discussionId, int employeeId)
        {
            // Валидация параметров
            if (discussionId <= 0)
            {
                MessageBox.Show("Неверный ID обсуждения");
                return false;
            }

            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Проверяем существование обсуждения
                        if (!DiscussionExists(discussionId, connection, transaction))
                        {
                            MessageBox.Show($"Обсуждение с ID {discussionId} не найдено");
                            transaction.Rollback();
                            return false;
                        }

                        // 2. Добавляем комментарий
                        var cmd = new NpgsqlCommand(
                            @"INSERT INTO discussion_comment 
                    (comment_text, creation_date, discussion_id, employee_id) 
                    VALUES (@text, CURRENT_TIMESTAMP, @disc_id, @emp_id)
                    RETURNING comment_id;", // Добавлено RETURNING для проверки
                            connection, transaction);

                        cmd.Parameters.AddWithValue("@text", commentText);
                        cmd.Parameters.AddWithValue("@disc_id", discussionId);
                        cmd.Parameters.AddWithValue("@emp_id", employeeId);

                        var newCommentId = cmd.ExecuteScalar();
                        if (newCommentId == null)
                        {
                            MessageBox.Show("Не удалось добавить комментарий");
                            transaction.Rollback();
                            return false;
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (NpgsqlException ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка: Не найдено обсуждение с ID {discussionId}");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка при добавлении комментария: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        private bool DiscussionExists(int discussionId, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            var cmd = new NpgsqlCommand(
                "SELECT 1 FROM discussion WHERE discussion_id = @disc_id LIMIT 1",
                connection, transaction);
            cmd.Parameters.AddWithValue("@disc_id", discussionId);
            return cmd.ExecuteScalar() != null;
        }

        public User GetEmployeeById(int employeeId)
        {
            User employee = null;

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sqlExp = @"SELECT * FROM public.get_employee_by_id(@p_employee_id)";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@p_employee_id", employeeId);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                employee = new User
                                {
                                    UserId = reader.GetInt32(reader.GetOrdinal("employee_id")),
                                    FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                                    LastName = reader.GetString(reader.GetOrdinal("last_name")),
                                    ImagePath = reader.GetString(reader.GetOrdinal("image_path"))
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