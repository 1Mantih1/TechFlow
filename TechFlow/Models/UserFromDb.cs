using System;
using System.Windows;
using TechFlow.Classes;
using TechFlow.Windows;
using Npgsql;
using System.Collections.Generic;
using System.IO;

namespace TechFlow.Models
{
    class UserFromDb
    {
        public User GetUser(string loginOrEmail, string password)
        {
            User user = null;
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connect.Open();
                    string sqlExp;
                    NpgsqlCommand cmd;

                    if (!loginOrEmail.Contains("@"))
                    {
                        sqlExp = @"
                    SELECT e.*, r.employee_role_name 
                    FROM employee e
                    JOIN employee_role r ON e.role_id = r.employee_role_id
                    WHERE e.login = @login";
                        cmd = new NpgsqlCommand(sqlExp, connect);
                        cmd.Parameters.AddWithValue("login", loginOrEmail);
                    }
                    else
                    {
                        sqlExp = @"
                    SELECT e.*, r.employee_role_name 
                    FROM employee e
                    JOIN employee_role r ON e.role_id = r.employee_role_id
                    WHERE e.email = @email";
                        cmd = new NpgsqlCommand(sqlExp, connect);
                        cmd.Parameters.AddWithValue("email", loginOrEmail);
                    }

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            if (!string.IsNullOrEmpty(password))
                            {
                                if (Verification.VerifySHA512Hash(password, reader["password"].ToString()))
                                {
                                    DateTime dateOfBirth = DateTime.MinValue;
                                    if (reader["date_of_birth"] != DBNull.Value)
                                    {
                                        dateOfBirth = Convert.ToDateTime(reader["date_of_birth"]);
                                    }

                                    string phone = reader["phone"] != DBNull.Value ? reader["phone"].ToString() : string.Empty;
                                    string address = reader["address"] != DBNull.Value ? reader["address"].ToString() : string.Empty;
                                    string imagePath = reader["image_path"] != DBNull.Value ? reader["image_path"].ToString() : string.Empty;

                                    user = new User(
                                        (int)reader["employee_id"],
                                        reader["first_name"].ToString(),
                                        reader["last_name"].ToString(),
                                        dateOfBirth,
                                        reader["login"].ToString(),
                                        reader["password"].ToString(),
                                        address,
                                        phone,
                                        (int)reader["role_id"],
                                        reader["email"].ToString(),
                                        imagePath
                                    );

                                    user.RoleName = reader["employee_role_name"].ToString();
                                }
                                else
                                {
                                    MessageBox.Show("Неверный пароль");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Пароль не может быть пустым");
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка при получении данных пользователя: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}");
            }

            return user;
        }


        public Client GetClient(string loginOrEmail, string password)
        {
            Client client = null;
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connect.Open();
                    string sqlExp;
                    NpgsqlCommand cmd;

                    if (!loginOrEmail.Contains("@"))
                    {
                        sqlExp = "SELECT * FROM client WHERE login=@login;";
                        cmd = new NpgsqlCommand(sqlExp, connect);
                        cmd.Parameters.AddWithValue("login", loginOrEmail);
                    }
                    else
                    {
                        sqlExp = "SELECT * FROM client WHERE email=@email;";
                        cmd = new NpgsqlCommand(sqlExp, connect);
                        cmd.Parameters.AddWithValue("email", loginOrEmail);
                    }

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            if (password != "")
                            {
                                if (Verification.VerifySHA512Hash(password, (string)reader["password_hash"]))
                                {
                                    string phone = reader["phone"] != DBNull.Value ? reader["phone"].ToString() : string.Empty;

                                    client = new Client(
                                        (int)reader["client_id"],
                                        reader["organization_name"].ToString(),
                                        reader["first_name"].ToString(),
                                        reader["last_name"].ToString(),
                                        reader["email"].ToString(),
                                        phone,
                                        reader["login"].ToString()
                                    );
                                }
                                else
                                {
                                    MessageBox.Show("Неверный пароль");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Пароль не может быть пустым");
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка при получении данных клиента: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}");
            }
            return client;
        }

        public bool f = false;
        public bool CheckPassword(string password, string passRepeat)
        {
            if (password.Length < 6)
            {
                MessageBox.Show("Длина пароля не может быть меньше 6 символов");
                return false;
            }
            else
            {
                bool f, f1, f2;
                f = f1 = f2 = false;
                for (int i = 0; i < password.Length; i++)
                {
                    if (Char.IsDigit(password[i])) f1 = true;
                    if (Char.IsUpper(password[i])) f2 = true;
                    if (f1 && f2) break;
                }
                if (!(f1 && f2))
                {
                    MessageBox.Show("Пароль должен содержать хотя бы одну цифру и одну заглавную букву!");
                    return f1 && f2;
                }
                else
                {
                    string symbol = "!@#$%^";

                    for (int i = 0; i < password.Length; i++)
                    {
                        for (int j = 0; j < symbol.Length; j++)
                        {
                            if (password[i] == symbol[j]) { f = true; break; }
                        }
                    }
                    if (!f)
                    {
                        MessageBox.Show("Пароль должен содержать один из символов !@#$%^");
                    }
                    if ((password == passRepeat) && f) return true;
                    else { MessageBox.Show("Неверно подтвержден пароль"); return false; }
                }
            }
        }

        public bool CheckUser(string login)
        {
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connect.Open();

                    string sqlExp = "SELECT login from employee where login=@login";
                    NpgsqlCommand cmd = new NpgsqlCommand(sqlExp, connect);
                    cmd.Parameters.AddWithValue("@login", login);
                    NpgsqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        MessageBox.Show("Такой логин уже есть");
                        return false;
                    }
                    else
                    {
                        reader.Close();
                        return true;
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public int AddEmployee(string login, string firstName, string lastName, string email, string password, string phone, string address, DateTime? dateOfBirth, int roleId)
        {
            try
            {
                string hashedPassword = Verification.GetSHA512Hash(password);
                using (NpgsqlConnection connect = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connect.Open();

                    string sqlExp = "INSERT INTO employee (login, first_name, last_name, email, password, phone, address, date_of_birth, role_id) " +
                                    "VALUES (@login, @firstName, @lastName, @email, @password, @phone, @address, @dateOfBirth, @roleId) RETURNING employee_id";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sqlExp, connect))
                    {
                        cmd.Parameters.AddWithValue("login", login);
                        cmd.Parameters.AddWithValue("firstName", firstName);
                        cmd.Parameters.AddWithValue("lastName", lastName);
                        cmd.Parameters.AddWithValue("email", email);
                        cmd.Parameters.AddWithValue("password", hashedPassword);
                        cmd.Parameters.AddWithValue("phone", phone ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("address", address ?? (object)DBNull.Value);

                        cmd.Parameters.AddWithValue("dateOfBirth", (object)dateOfBirth ?? DBNull.Value);

                        cmd.Parameters.AddWithValue("roleId", roleId);

                        cmd.Parameters["dateOfBirth"].NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Date;

                        int employeeId = (int)cmd.ExecuteScalar();
                        MessageBox.Show("Сотрудник успешно добавлен!");
                        return employeeId;

                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при добавлении сотрудника: " + ex.Message);
                return -1;
            }
        }

        public bool CheckEmail(string email)
        {
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connect.Open();

                    string sqlExp = "SELECT email from employee where email=@email";
                    NpgsqlCommand cmd = new NpgsqlCommand(sqlExp, connect);
                    cmd.Parameters.AddWithValue("@email", email);
                    NpgsqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        MessageBox.Show("Такой email уже есть");
                        return false;
                    }
                    else
                    {
                        reader.Close();
                        return true;
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public bool UpdateEmployeeProfile(User employee)
        {
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connect.Open();

                    string sqlExp = "UPDATE employee SET first_name = @firstName, last_name = @lastName, " +
                                    "email = @email, phone = @phone, address = @address, date_of_birth = @dateOfBirth, " +
                                    "role_id = @roleId WHERE employee_id = @id";

                    NpgsqlCommand cmd = new NpgsqlCommand(sqlExp, connect);
                    cmd.Parameters.AddWithValue("firstName", employee.FirstName);
                    cmd.Parameters.AddWithValue("lastName", employee.LastName);
                    cmd.Parameters.AddWithValue("email", employee.Email);
                    cmd.Parameters.AddWithValue("phone", employee.Phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("address", employee.Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("dateOfBirth", employee.DateOfBirth);
                    cmd.Parameters.AddWithValue("roleId", employee.RoleId);
                    cmd.Parameters.AddWithValue("id", employee.UserId);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при обновлении профиля сотрудника: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool AddOrUpdateImagePath(int employee_id, string imagePath)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    string fileName = Path.GetFileName(imagePath);

                    string relativePath = Path.Combine("..", "..", "avatar", fileName);

                    string sqlExp = @"
                UPDATE employee
                SET image_path = @ImagePath
                WHERE employee_id = @employee_id;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExp, connection))
                    {
                        command.Parameters.AddWithValue("@employee_id", employee_id);
                        command.Parameters.AddWithValue("@ImagePath", relativePath);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка обновления аватара: " + ex.Message);
                    return false;
                }
            }
        }


        public bool UpdatePassword(string email, string newPassword)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string hashedPassword = Verification.GetSHA512Hash(newPassword);

                    string checkEmployeeSql = "SELECT COUNT(*) FROM employee WHERE email = @email";
                    bool isEmployee = false;

                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkEmployeeSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@email", email);
                        isEmployee = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;
                    }

                    string updateSql;
                    if (isEmployee)
                    {
                        updateSql = "UPDATE employee SET password_hash = @password WHERE email = @email";
                    }
                    else
                    {
                        updateSql = "UPDATE client SET password_hash = @password WHERE email = @email";
                    }

                    using (NpgsqlCommand updateCommand = new NpgsqlCommand(updateSql, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@email", email);
                        updateCommand.Parameters.AddWithValue("@password", hashedPassword);

                        int rowsAffected = updateCommand.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Ошибка обновления пароля: " + ex.Message);
                    return false;
                }
            }
        }

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

        public bool SetEmployeeRole(int employeeId)
        {
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connect.Open();

                    string roleQuery = "SELECT employee_role_id FROM employee_role WHERE employee_role_name = 'Сотрудник'";
                    NpgsqlCommand roleCmd = new NpgsqlCommand(roleQuery, connect);
                    var roleId = roleCmd.ExecuteScalar();

                    if (roleId == null)
                    {
                        MessageBox.Show("Роль 'Не определено' не найдена в системе");
                        return false;
                    }

                    string updateQuery = "UPDATE employee SET role_id = @roleId WHERE employee_id = @employeeId";
                    NpgsqlCommand updateCmd = new NpgsqlCommand(updateQuery, connect);
                    updateCmd.Parameters.AddWithValue("@roleId", roleId);
                    updateCmd.Parameters.AddWithValue("@employeeId", employeeId);

                    int rowsAffected = updateCmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при установке роли 'Не определено': " + ex.Message);
                return false;
            }
        }

        public bool DeleteUser(int employeeId)
        {
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connect.Open();

                    string deleteCustomerQuery = "DELETE FROM customer WHERE employee_id = @employeeId";
                    NpgsqlCommand deleteCustomerCmd = new NpgsqlCommand(deleteCustomerQuery, connect);
                    deleteCustomerCmd.Parameters.AddWithValue("@employeeId", employeeId);
                    deleteCustomerCmd.ExecuteNonQuery();

                    string deleteEmployeeQuery = "DELETE FROM employee WHERE employee_id = @employeeId";
                    NpgsqlCommand deleteEmployeeCmd = new NpgsqlCommand(deleteEmployeeQuery, connect);
                    deleteEmployeeCmd.Parameters.AddWithValue("@employeeId", employeeId);

                    int rowsAffected = deleteEmployeeCmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка при удалении сотрудника: {ex.Message}");
                return false;
            }
        }

        public bool RegisterClient(string login, string firstName, string lastName,
                                 string email, string phone, string organizationName,
                                 string password)
        {
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connect.Open();

                    string hashedPassword = Verification.GetSHA512Hash(password);

                    string query = @"INSERT INTO client (login, organization_name, first_name, 
                           last_name, email, phone, password_hash) 
                           VALUES (@login, @orgName, @firstName, @lastName, 
                           @email, @phone, @password)";

                    NpgsqlCommand cmd = new NpgsqlCommand(query, connect);
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@orgName", organizationName);
                    cmd.Parameters.AddWithValue("@firstName", firstName);
                    cmd.Parameters.AddWithValue("@lastName", lastName);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@phone", phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при регистрации клиента: " + ex.Message);
                return false;
            }
        }

        public bool IsLoginExists(string login)
        {
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connect.Open();

                    string query = @"SELECT COUNT(*) FROM employee WHERE login = @login
                           UNION ALL
                           SELECT COUNT(*) FROM client WHERE login = @login";

                    NpgsqlCommand cmd = new NpgsqlCommand(query, connect);
                    cmd.Parameters.AddWithValue("@login", login);

                    using (var reader = cmd.ExecuteReader())
                    {
                        int total = 0;
                        while (reader.Read())
                        {
                            total += reader.GetInt32(0);
                        }
                        return total > 0;
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при проверке логина: " + ex.Message);
                return true;
            }
        }

        public bool IsEmailExists(string email)
        {
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connect.Open();

                    string query = @"SELECT COUNT(*) FROM employee WHERE email = @email
                           UNION ALL
                           SELECT COUNT(*) FROM client WHERE email = @email";

                    NpgsqlCommand cmd = new NpgsqlCommand(query, connect);
                    cmd.Parameters.AddWithValue("@email", email);

                    using (var reader = cmd.ExecuteReader())
                    {
                        int total = 0;
                        while (reader.Read())
                        {
                            total += reader.GetInt32(0);
                        }
                        return total > 0;
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при проверке email: " + ex.Message);
                return true;
            }
        }

        public bool IsPendingEmployee(int userId)
        {
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(DbConnection.connectionStr))
                {
                    connect.Open();

                    string query = @"SELECT er.employee_role_name 
                       FROM employee e
                       JOIN employee_role er ON e.role_id = er.employee_role_id
                       WHERE e.employee_id = @userId";

                    NpgsqlCommand cmd = new NpgsqlCommand(query, connect);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    var roleName = cmd.ExecuteScalar()?.ToString();
                    return roleName == "Сотрудник";
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при проверке статуса: " + ex.Message);
                return true;
            }
        }

        public List<User> LoadEmployeesWithDefaultRole()
        {
            List<User> employees = new List<User>();

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();
                    string sqlQuery = @"
                SELECT e.employee_id, e.first_name, e.last_name, e.date_of_birth, 
                       e.login, e.phone, e.address, e.role_id, e.email, e.image_path,
                       er.employee_role_name
                FROM employee e
                JOIN employee_role er ON e.role_id = er.employee_role_id
                WHERE er.employee_role_name = 'Сотрудник'
                ORDER BY e.last_name, e.first_name;";

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new User
                            {
                                UserId = Convert.ToInt32(reader["employee_id"]),
                                FirstName = reader["first_name"].ToString(),
                                LastName = reader["last_name"].ToString(),
                                DateOfBirth = reader["date_of_birth"] != DBNull.Value ? Convert.ToDateTime(reader["date_of_birth"]) : DateTime.MinValue,
                                Login = reader["login"].ToString(),
                                Phone = reader["phone"] != DBNull.Value ? reader["phone"].ToString() : null,
                                Address = reader["address"] != DBNull.Value ? reader["address"].ToString() : null,
                                RoleId = Convert.ToInt32(reader["role_id"]),
                                RoleName = reader["employee_role_name"].ToString(),
                                Email = reader["email"].ToString(),
                                ImagePath = reader["image_path"] != DBNull.Value ? reader["image_path"].ToString() : "../image/man1.png"
                            });
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка при загрузке сотрудников: {ex.Message}");
                }
            }

            return employees;
        }

        public bool UpdateEmployeeRole(int employeeId, string targetRoleName)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.connectionStr))
            {
                try
                {
                    connection.Open();

                    string getRoleIdQuery = @"SELECT employee_role_id 
                                    FROM employee_role 
                                    WHERE employee_role_name = @roleName 
                                    LIMIT 1;";

                    int targetRoleId;

                    using (NpgsqlCommand getRoleCmd = new NpgsqlCommand(getRoleIdQuery, connection))
                    {
                        getRoleCmd.Parameters.AddWithValue("@roleName", targetRoleName);
                        var result = getRoleCmd.ExecuteScalar();

                        if (result == null || result == DBNull.Value)
                        {
                            MessageBox.Show($"Роль '{targetRoleName}' не найдена в системе");
                            return false;
                        }
                        targetRoleId = Convert.ToInt32(result);
                    }

                    string updateQuery = @"UPDATE employee 
                                 SET role_id = @roleId 
                                 WHERE employee_id = @employeeId;";

                    using (NpgsqlCommand updateCmd = new NpgsqlCommand(updateQuery, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@roleId", targetRoleId);
                        updateCmd.Parameters.AddWithValue("@employeeId", employeeId);

                        return updateCmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show($"Ошибка при изменении роли: {ex.Message}");
                    return false;
                }
            }
        }
    }
}
