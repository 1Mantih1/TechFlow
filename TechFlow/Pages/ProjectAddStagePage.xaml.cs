using System;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using TechFlow.Models;
using TechFlow.Classes;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    public partial class ProjectAddStagePage : Page
    {
        public ProjectAddStagePage()
        {
            InitializeComponent();
            LoadProjects();
            LoadStatuses();
        }

        private void LoadProjects()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connection.Open();
                    string sql = "SELECT project_id, project_name FROM project ORDER BY project_name";
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProjectComboBox.Items.Add(new
                            {
                                ProjectId = reader.GetInt32(0),
                                ProjectName = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки проектов: {ex.Message}");
            }
        }

        private void LoadStatuses()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connection.Open();
                    string sql = "SELECT status_id, status_name FROM status ORDER BY status_name";
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            StatusComboBox.Items.Add(new
                            {
                                StatusId = reader.GetInt32(0),
                                StatusName = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки статусов: {ex.Message}");
            }
        }

        private void AddStageButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StageNameField.Text))
            {
                CustomMessageBox.Show("Введите название стадии!");
                return;
            }

            if (ProjectComboBox.SelectedItem == null)
            {
                CustomMessageBox.Show("Выберите проект!");
                return;
            }

            if (StatusComboBox.SelectedItem == null)
            {
                CustomMessageBox.Show("Выберите статус!");
                return;
            }

            if (StartDateField.SelectedDate == null)
            {
                CustomMessageBox.Show("Выберите дату начала!");
                return;
            }

            try
            {
                dynamic selectedProject = ProjectComboBox.SelectedItem;
                dynamic selectedStatus = StatusComboBox.SelectedItem;

                using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connection.Open();
                    string sql = @"INSERT INTO project_stage (
                                    stage_name, 
                                    project_stage_description, 
                                    start_date, 
                                    end_date, 
                                    status_id, 
                                    project_id
                                ) VALUES (
                                    @stage_name, 
                                    @description, 
                                    @start_date, 
                                    @end_date, 
                                    @status_id, 
                                    @project_id
                                )";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@stage_name", StageNameField.Text);
                        command.Parameters.AddWithValue("@description", DescriptionField.Text ?? string.Empty);
                        command.Parameters.AddWithValue("@start_date", StartDateField.SelectedDate.Value);
                        command.Parameters.AddWithValue("@end_date", EndDateField.SelectedDate ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@status_id", selectedStatus.StatusId);
                        command.Parameters.AddWithValue("@project_id", selectedProject.ProjectId);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            CustomMessageBox.Show("Стадия проекта успешно добавлена!");
                            ClearForm();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при добавлении стадии: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            StageNameField.Text = string.Empty;
            DescriptionField.Text = string.Empty;
            ProjectComboBox.SelectedIndex = -1;
            StatusComboBox.SelectedIndex = -1;
            StartDateField.SelectedDate = DateTime.Today;
            EndDateField.SelectedDate = DateTime.Today; 
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}