using System;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    public partial class AddTaskPage : Page
    {
        public AddTaskPage()
        {
            InitializeComponent();
            LoadStatuses();
            LoadStages();
            LoadTeams();
            StartDateField.SelectedDate = DateTime.Today;
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

        private void LoadStages()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connection.Open();
                    string sql = "SELECT stage_id, stage_name FROM project_stage ORDER BY stage_name";
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            StageComboBox.Items.Add(new
                            {
                                StageId = reader.GetInt32(0),
                                StageName = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки этапов: {ex.Message}");
            }
        }

        private void LoadTeams()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connection.Open();
                    string sql = "SELECT team_id, team_name FROM team ORDER BY team_name";
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TeamComboBox.Items.Add(new
                            {
                                TeamId = reader.GetInt32(0),
                                TeamName = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки команд: {ex.Message}");
            }
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskNameField.Text))
            {
                CustomMessageBox.Show("Введите название задачи!");
                return;
            }

            if (StatusComboBox.SelectedItem == null)
            {
                CustomMessageBox.Show("Выберите статус!");
                return;
            }

            if (StageComboBox.SelectedItem == null)
            {
                CustomMessageBox.Show("Выберите этап проекта!");
                return;
            }

            if (TeamComboBox.SelectedItem == null)
            {
                CustomMessageBox.Show("Выберите команду!");
                return;
            }

            if (StartDateField.SelectedDate == null)
            {
                CustomMessageBox.Show("Выберите дату начала!");
                return;
            }

            try
            {
                dynamic selectedStatus = StatusComboBox.SelectedItem;
                dynamic selectedStage = StageComboBox.SelectedItem;
                dynamic selectedTeam = TeamComboBox.SelectedItem;

                using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connection.Open();
                    string sql = @"INSERT INTO task (
                                task_name, 
                                task_description, 
                                start_date, 
                                end_date, 
                                status_id, 
                                stage_id, 
                                team_id
                            ) VALUES (
                                @task_name, 
                                @description, 
                                @start_date, 
                                @end_date, 
                                @status_id, 
                                @stage_id, 
                                @team_id
                            )";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@task_name", TaskNameField.Text);
                        command.Parameters.AddWithValue("@description", DescriptionField.Text ?? string.Empty);
                        command.Parameters.AddWithValue("@start_date", StartDateField.SelectedDate.Value);
                        command.Parameters.AddWithValue("@end_date", EndDateField.SelectedDate ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@status_id", selectedStatus.StatusId);
                        command.Parameters.AddWithValue("@stage_id", selectedStage.StageId);
                        command.Parameters.AddWithValue("@team_id", selectedTeam.TeamId);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            CustomMessageBox.Show("Задача успешно добавлена!");
                            ClearForm();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при добавлении задачи: {ex.Message}");
            }
        }


        private void ClearForm()
        {
            TaskNameField.Text = string.Empty;
            DescriptionField.Text = string.Empty;
            StatusComboBox.SelectedIndex = -1;
            StageComboBox.SelectedIndex = -1;
            TeamComboBox.SelectedIndex = -1;
            StartDateField.SelectedDate = DateTime.Today;
            EndDateField.SelectedDate = null;
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