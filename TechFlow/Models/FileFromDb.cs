using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using TechFlow.Classes;

namespace TechFlow.Models
{
    public class FileFromDb
    {
        private readonly string _attachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../Attachments");

        public FileFromDb()
        {
            if (!Directory.Exists(_attachmentsFolder))
            {
                Directory.CreateDirectory(_attachmentsFolder);
            }
        }

        public void AddFile(int taskId, int employeeId, string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var uniqueFileName = $"{Guid.NewGuid()}{fileInfo.Extension}";
                var destinationPath = Path.Combine(_attachmentsFolder, uniqueFileName);

                File.Copy(filePath, destinationPath);

                using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connection.Open();
                    var sqlExp = @"INSERT INTO task_file 
                                (file_name, file_path, file_size, file_type, task_id, employee_id) 
                                VALUES (@name, @path, @size, @type, @taskId, @employeeId)";

                    using (var command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.Parameters.AddWithValue("@name", fileInfo.Name);
                        command.Parameters.AddWithValue("@path", destinationPath);
                        command.Parameters.AddWithValue("@size", fileInfo.Length);
                        command.Parameters.AddWithValue("@type", fileInfo.Extension);
                        command.Parameters.AddWithValue("@taskId", taskId);
                        command.Parameters.AddWithValue("@employeeId", employeeId);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении файла: {ex.Message}");
            }
        }

        public List<TaskFile> LoadFiles(int taskId)
        {
            var files = new List<TaskFile>();

            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    var sqlExp = @"SELECT * FROM task_file WHERE task_id = @taskId ORDER BY creation_date DESC";

                    using (var command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.Parameters.AddWithValue("@taskId", taskId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                files.Add(new TaskFile(
                                    Convert.ToInt32(reader["file_id"]),
                                    reader["file_name"].ToString(),
                                    reader["file_path"].ToString(),
                                    Convert.ToInt64(reader["file_size"]),
                                    reader["file_type"].ToString(),
                                    Convert.ToDateTime(reader["creation_date"]),
                                    Convert.ToInt32(reader["task_id"]),
                                    Convert.ToInt32(reader["employee_id"])
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки файлов: " + ex.Message);
                }
            }

            return files;
        }

        public void DownloadFile(string filePath, string suggestedFileName)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = suggestedFileName,
                    Filter = "All files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.Copy(filePath, saveFileDialog.FileName, overwrite: true);
                    MessageBox.Show("Файл успешно скачан!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при скачивании файла: {ex.Message}");
            }
        }
    }
}
