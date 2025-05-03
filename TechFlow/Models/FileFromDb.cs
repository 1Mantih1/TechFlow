using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
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
                    var sqlExp = @"CALL public.insert_task_file(@p_file_name, @p_file_path, @p_file_size, @p_file_type, @p_task_id, @p_employee_id)";

                    using (var command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@p_file_name", fileInfo.Name);
                        command.Parameters.AddWithValue("@p_file_path", destinationPath);
                        command.Parameters.AddWithValue("@p_file_size", fileInfo.Length);
                        command.Parameters.AddWithValue("@p_file_type", fileInfo.Extension);
                        command.Parameters.AddWithValue("@p_task_id", taskId);
                        command.Parameters.AddWithValue("@p_employee_id", employeeId);

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
                    var sqlExp = @"SELECT * FROM public.get_task_files_by_task_id_v2(@p_task_id)";

                    using (var command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@p_task_id", taskId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                files.Add(new TaskFile(
                                    reader.GetInt32(reader.GetOrdinal("out_file_id")),
                                    reader.GetString(reader.GetOrdinal("out_file_name")),
                                    reader.GetString(reader.GetOrdinal("out_file_path")),
                                    reader.GetInt64(reader.GetOrdinal("out_file_size")),
                                    reader.GetString(reader.GetOrdinal("out_file_type")),
                                    reader.GetDateTime(reader.GetOrdinal("out_creation_date")),
                                    reader.GetInt32(reader.GetOrdinal("out_task_id")),
                                    reader.GetInt32(reader.GetOrdinal("out_employee_id"))
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