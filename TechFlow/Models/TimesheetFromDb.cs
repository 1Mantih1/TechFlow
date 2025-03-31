using Npgsql;
using System;
using System.Collections.Generic;
using System.Windows;
using TechFlow.Classes;
using TechFlow.Windows;

namespace TechFlow.Models
{
    public class TimesheetFromDb
    {
        public List<Timesheet> LoadTimesheets()
        {
            List<Timesheet> timesheets = new List<Timesheet>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    string sqlExp = @"
                        SELECT t.timesheet_id, t.work_date, t.employee_id, ws.status_code, ws.description
                        FROM public.timesheet t
                        JOIN public.work_status ws ON t.code_id = ws.id
                        ORDER BY t.timesheet_id;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            WorkStatus status = new WorkStatus(
                                reader["status_code"].ToString(),
                                reader["description"].ToString()
                            );

                            timesheets.Add(new Timesheet(
                                Convert.ToInt32(reader["timesheet_id"]),
                                Convert.ToDateTime(reader["work_date"]),
                                Convert.ToInt32(reader["employee_id"]),
                                status
                            ));
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки табелей: " + ex.Message);
                }
            }

            return timesheets;
        }

        public void UpdateStatus(string status)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    // Проверяем, есть ли уже запись за сегодня
                    string checkQuery = @"
                SELECT COUNT(*) FROM public.timesheet 
                WHERE employee_id = @employeeId AND work_date = @workDate;";

                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@employeeId", Authorization.currentUser.UserId);
                        checkCommand.Parameters.AddWithValue("@workDate", DateTime.Now.Date);

                        int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (count > 0)
                        {
                            CustomMessageBox.Show("Вы уже отмечались сегодня. Повторная отметка невозможна.");
                            return; // Прерываем выполнение метода
                        }
                    }

                    // Если отметки ещё нет, выполняем вставку
                    string sqlExp = @"
                INSERT INTO public.timesheet (employee_id, work_date, code_id)
                SELECT @employeeId, @workDate, ws.id 
                FROM public.work_status ws
                WHERE ws.description = @status;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.Parameters.AddWithValue("@status", status);
                        command.Parameters.AddWithValue("@employeeId", Authorization.currentUser.UserId);
                        command.Parameters.AddWithValue("@workDate", DateTime.Now.Date);

                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Вы успешно отметились!");
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка добавления записи: " + ex.Message);
                }
            }
        }
    }
}
