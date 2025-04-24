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
        public bool IsAdmin(int employeeId)
        {
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connect.Open();

                    string sqlExp = @"SELECT 1 FROM employee e
                                    JOIN employee_role er ON e.role_id = er.employee_role_id
                                    WHERE e.employee_id = @employeeId 
                                    AND er.employee_role_name = 'Администратор'";

                    NpgsqlCommand cmd = new NpgsqlCommand(sqlExp, connect);
                    cmd.Parameters.AddWithValue("@employeeId", employeeId);

                    return cmd.ExecuteScalar() != null;
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при проверке роли администратора: " + ex.Message);
                return false;
            }
        }

        public List<Timesheet> LoadTimesheets()
        {
            List<Timesheet> timesheets = new List<Timesheet>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    string sqlExp = @"
                        SELECT t.timesheet_id, t.work_date, t.employee_id, t.check_in_time, ws.status_code, ws.description
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
                                status,
                                reader["check_in_time"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["check_in_time"]) : null
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

        public DateTime? GetCheckInTime(int employeeId, DateTime date)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    string sqlExp = @"
                        SELECT check_in_time FROM public.timesheet 
                        WHERE employee_id = @employeeId 
                        AND work_date = @workDate
                        AND check_in_time IS NOT NULL;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        command.Parameters.AddWithValue("@workDate", date.Date);

                        var result = command.ExecuteScalar();
                        return result != DBNull.Value && result != null ? (DateTime?)Convert.ToDateTime(result) : null;
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка получения времени отметки: " + ex.Message);
                    return null;
                }
            }
        }

        public void UpdateStatus(string status, DateTime checkInTime)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

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
                            return;
                        }
                    }

                    string sqlExp = @"
                        INSERT INTO public.timesheet (employee_id, work_date, code_id, check_in_time)
                        SELECT @employeeId, @workDate, ws.id, @checkInTime
                        FROM public.work_status ws
                        WHERE ws.description = @status;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.Parameters.AddWithValue("@status", status);
                        command.Parameters.AddWithValue("@employeeId", Authorization.currentUser.UserId);
                        command.Parameters.AddWithValue("@workDate", DateTime.Now.Date);
                        command.Parameters.AddWithValue("@checkInTime", checkInTime);

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

        public void CreateInitialTimesheet(int employeeId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    string sqlExp = @"
                        INSERT INTO public.timesheet (employee_id)
                        VALUES (@employeeId);";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        command.ExecuteNonQuery();
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка создания табеля: " + ex.Message);
                }
            }
        }
    }
}