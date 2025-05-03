using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TechFlow.Classes;
using TechFlow.Windows;

namespace TechFlow.Models
{
    public class TimesheetFromDb
    {
//        public bool IsAdmin(int employeeId)
//        {
//            try
//            {
//                using (NpgsqlConnection connect = new NpgsqlConnection(DbConnection.connectionStr))
//                {
//                    connect.Open();

//                    // Updated query - adjust column names as per your actual schema
//                    string sqlExp = @"SELECT 1
//FROM employee e
//JOIN employee_role er ON e.role_id = er.employee_role_id
//WHERE e.employee_id = @employeeId
//  AND er.employee_role_name = 'Администратор';

//";

//                    NpgsqlCommand cmd = new NpgsqlCommand(sqlExp, connect);
//                    cmd.Parameters.AddWithValue("@employeeId", employeeId);

//                    return cmd.ExecuteScalar() != null;
//                }
//            }
//            catch (NpgsqlException ex)
//            {
//                MessageBox.Show("Ошибка при проверке роли администратора: " + ex.Message);
//                return false;
//            }
//        }

        public List<Timesheet> LoadTimesheets()
        {
            var timesheets = new List<Timesheet>();

            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    const string sql = @"
                SELECT 
                    t.timesheet_id, 
                    t.work_date, 
                    t.employee_id, 
                    ws.status_code, 
                    ws.description,
                    t.start_time,
                    t.end_time
                FROM public.timesheet t
                JOIN public.work_status ws ON t.code_id = ws.id
                ORDER BY t.timesheet_id;";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var status = new WorkStatus(
                                reader.GetString(reader.GetOrdinal("status_code")),
                                reader.GetString(reader.GetOrdinal("description"))
                            );

                            var startTimeOrdinal = reader.GetOrdinal("start_time");
                            TimeSpan startTime = reader.IsDBNull(startTimeOrdinal)
                                ? TimeSpan.Zero
                                : reader.GetTimeSpan(startTimeOrdinal);

                            var endTimeOrdinal = reader.GetOrdinal("end_time");
                            TimeSpan endTime = reader.IsDBNull(endTimeOrdinal)
                                ? TimeSpan.Zero
                                : reader.GetTimeSpan(endTimeOrdinal);

                            timesheets.Add(new Timesheet(
                                reader.GetInt32(reader.GetOrdinal("timesheet_id")),
                                reader.GetDateTime(reader.GetOrdinal("work_date")),
                                reader.GetInt32(reader.GetOrdinal("employee_id")),
                                status,
                                startTime,
                                endTime
                            ));
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка загрузки табелей: {ex.Message}");
                }
            }

            return timesheets;
        }

        public void UpdateStatus(string status, TimeSpan startTime, TimeSpan endTime)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    // Проверка на существование записи
                    string checkQuery = @"
                SELECT COUNT(*) 
                FROM public.timesheet 
                WHERE employee_id = @employeeId AND work_date = @workDate;";

                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@employeeId", Authorization.currentUser.UserId);
                        checkCommand.Parameters.AddWithValue("@workDate", DateTime.Now.Date);

                        int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (count > 0)
                        {
                            CustomMessageBox.Show("Вы уже отметились сегодня. Повторная отметка невозможна.");
                            return;
                        }
                    }

                    // Вставка записи в таблицу
                    string sqlExp = @"
                INSERT INTO public.timesheet 
                    (employee_id, work_date, code_id, start_time, end_time)
                SELECT 
                    @employeeId, @workDate, ws.id, @startTime, @endTime
                FROM public.work_status ws
                WHERE ws.description = @status;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.Parameters.AddWithValue("@status", status);
                        command.Parameters.AddWithValue("@employeeId", Authorization.currentUser.UserId);
                        command.Parameters.AddWithValue("@workDate", DateTime.Now.Date);
                        command.Parameters.AddWithValue("@startTime", startTime);
                        command.Parameters.AddWithValue("@endTime", endTime);

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

        public List<WorkStatus> LoadWorkStatuses()
        {
            var statuses = new List<WorkStatus>();

            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    const string sql = "SELECT status_code, description FROM work_status ORDER BY id";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            statuses.Add(new WorkStatus(
                                reader.GetString(reader.GetOrdinal("status_code")),
                                reader.GetString(reader.GetOrdinal("description"))
                            ));
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}");
                }
            }

            return statuses;
        }

        public List<Timesheet> LoadEmployeeTimesheets(int employeeId)
        {
            var timesheets = new List<Timesheet>();

            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    const string sql = @"
            SELECT 
                t.work_date, 
                ws.status_code
            FROM timesheet t
            JOIN work_status ws ON t.code_id = ws.id
            WHERE t.employee_id = @employeeId
            AND EXTRACT(YEAR FROM t.work_date) = EXTRACT(YEAR FROM CURRENT_DATE)
            ORDER BY t.work_date";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@employeeId", employeeId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                timesheets.Add(new Timesheet(
                                    0,
                                    reader.GetDateTime(reader.GetOrdinal("work_date")),
                                    employeeId,
                                    new WorkStatus(
                                        reader.GetString(reader.GetOrdinal("status_code")),
                                        string.Empty), // Description not needed
                                    TimeSpan.Zero,
                                    TimeSpan.Zero
                                ));
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка загрузки графика сотрудника: {ex.Message}");
                }
            }

            return timesheets;
        }

        public bool SaveTimesheetChanges(int employeeId, List<TimesheetRecord> newRecords)
        {
            // Валидация входных данных
            if (newRecords == null || !newRecords.Any())
            {
                MessageBox.Show("Нет данных для сохранения");
                return false;
            }

            using (var connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    int currentYear = DateTime.Now.Year;

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Словарь для хранения существующих записей (ключ - день и месяц)
                            var existingRecords = new Dictionary<(int Day, int Month), string>();

                            // Загрузка существующих записей
                            string selectSql = @"
                        SELECT 
                            EXTRACT(DAY FROM work_date)::int as day,
                            EXTRACT(MONTH FROM work_date)::int as month,
                            ws.status_code
                        FROM timesheet t
                        JOIN work_status ws ON t.code_id = ws.id
                        WHERE t.employee_id = @employeeId
                        AND EXTRACT(YEAR FROM work_date) = @year";

                            using (var selectCmd = new NpgsqlCommand(selectSql, connection, transaction))
                            {
                                selectCmd.Parameters.AddWithValue("@employeeId", employeeId);
                                selectCmd.Parameters.AddWithValue("@year", currentYear);

                                using (var reader = selectCmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        int day = reader.GetInt32(0);
                                        int month = reader.GetInt32(1);
                                        string statusCode = reader.GetString(2);
                                        existingRecords[(day, month)] = statusCode;
                                    }
                                }
                            }

                            // Обработка новых записей
                            foreach (var record in newRecords)
                            {
                                try
                                {
                                    // Пропускаем записи с пустым статусом
                                    if (string.IsNullOrWhiteSpace(record.StatusCode))
                                        continue;

                                    // Проверка валидности даты
                                    if (record.Day < 1 || record.Day > 31 || record.Month < 1 || record.Month > 12)
                                    {
                                        MessageBox.Show($"Некорректная дата: день {record.Day}, месяц {record.Month}");
                                        continue;
                                    }

                                    var key = (record.Day, record.Month);

                                    if (existingRecords.ContainsKey(key))
                                    {
                                        // Обновляем только если статус изменился
                                        if (existingRecords[key] != record.StatusCode)
                                        {
                                            UpdateRecord(connection, transaction, employeeId, record, currentYear);
                                        }
                                    }
                                    else
                                    {
                                        // Добавляем новую запись
                                        InsertRecord(connection, transaction, employeeId, record, currentYear);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Ошибка обработки записи (день {record.Day}, месяц {record.Month}): {ex.Message}");
                                }
                            }

                            // Явное подтверждение транзакции
                            transaction.Commit();
                            MessageBox.Show("Данные успешно сохранены!");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Ошибка при сохранении: {ex.Message}\n\n{ex.StackTrace}");
                            return false;
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}");
                    return false;
                }
            }
        }

        private void InsertRecord(NpgsqlConnection connection, NpgsqlTransaction transaction,
                                int employeeId, TimesheetRecord record, int year)
        {
            string sql = @"
        INSERT INTO timesheet (employee_id, work_date, code_id)
        VALUES (
            @employeeId, 
            MAKE_DATE(@year, @month, @day),
            (SELECT id FROM work_status WHERE status_code = @statusCode)
        )";

            using (var cmd = new NpgsqlCommand(sql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@employeeId", employeeId);
                cmd.Parameters.AddWithValue("@day", record.Day);
                cmd.Parameters.AddWithValue("@month", record.Month);
                cmd.Parameters.AddWithValue("@year", year);
                cmd.Parameters.AddWithValue("@statusCode", record.StatusCode);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new Exception("Не удалось добавить запись (возможно, неверный код статуса)");
                }
            }
        }

        private void UpdateRecord(NpgsqlConnection connection, NpgsqlTransaction transaction,
                                int employeeId, TimesheetRecord record, int year)
        {
            string sql = @"
        UPDATE timesheet 
        SET code_id = (SELECT id FROM work_status WHERE status_code = @statusCode)
        WHERE employee_id = @employeeId
        AND work_date = MAKE_DATE(@year, @month, @day)";

            using (var cmd = new NpgsqlCommand(sql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@employeeId", employeeId);
                cmd.Parameters.AddWithValue("@day", record.Day);
                cmd.Parameters.AddWithValue("@month", record.Month);
                cmd.Parameters.AddWithValue("@year", year);
                cmd.Parameters.AddWithValue("@statusCode", record.StatusCode);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new Exception("Не удалось обновить запись");
                }
            }
        }
    }
}