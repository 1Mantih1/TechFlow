using System;
using System.Net;
using System.Net.Mail;
using System.Windows;

namespace TechFlow.Classes
{
    class EmailSender
    {
        private string smtpServer = "smtp.yandex.ru";
        private int smtpPort = 587;
        private string senderEmail = "grigorievnicita@yandex.ru";
        private string senderPassword = "ygagpzankybogbra";

        public void SendVerificationCode(string recipientEmail, string verificationCode)
        {
            try
            {
                string htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{
                            font-family: 'Segoe UI', Arial, sans-serif;
                            background-color: #f5f5f5;
                            color: #333;
                            line-height: 1.6;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 0 auto;
                            padding: 20px;
                            background-color: #fff;
                            border-radius: 8px;
                            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                        }}
                        .header {{
                            background: linear-gradient(135deg, #5fc4b8, #3d8f7a);
                            color: white;
                            padding: 20px;
                            text-align: center;
                            border-radius: 8px 8px 0 0;
                            margin-bottom: 20px;
                        }}
                        .code {{
                            font-size: 24px;
                            font-weight: bold;
                            text-align: center;
                            margin: 20px 0;
                            padding: 15px;
                            background-color: #f0f0f0;
                            border-radius: 5px;
                            letter-spacing: 3px;
                        }}
                        .footer {{
                            margin-top: 30px;
                            text-align: center;
                            font-size: 12px;
                            color: #777;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>TechFlow - Подтверждение регистрации</h1>
                        </div>
                        <p>Благодарим вас за регистрацию в TechFlow. Для завершения регистрации используйте следующий код подтверждения:</p>
                        <div class='code'>{verificationCode}</div>
                        <p>Код действителен в течение 15 минут. Если вы не запрашивали регистрацию, проигнорируйте это письмо.</p>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} TechFlow. Все права защищены.</p>
                        </div>
                    </div>
                </body>
                </html>";

                SendEmail(recipientEmail, "Код подтверждения TechFlow", htmlBody);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке письма: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public bool SendPasswordRecoveryCode(string recipientEmail, string verificationCode)
        {
            try
            {
                string htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{
                            font-family: 'Segoe UI', Arial, sans-serif;
                            background-color: #f5f5f5;
                            color: #333;
                            line-height: 1.6;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 0 auto;
                            padding: 20px;
                            background-color: #fff;
                            border-radius: 8px;
                            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                        }}
                        .header {{
                            background: linear-gradient(135deg, #5fc4b8, #3d8f7a);
                            color: white;
                            padding: 20px;
                            text-align: center;
                            border-radius: 8px 8px 0 0;
                            margin-bottom: 20px;
                        }}
                        .code {{
                            font-size: 24px;
                            font-weight: bold;
                            text-align: center;
                            margin: 20px 0;
                            padding: 15px;
                            background-color: #f0f0f0;
                            border-radius: 5px;
                            letter-spacing: 3px;
                        }}
                        .footer {{
                            margin-top: 30px;
                            text-align: center;
                            font-size: 12px;
                            color: #777;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Восстановление пароля</h1>
                        </div>
                        <p>Вы запросили сброс пароля для вашей учетной записи TechFlow. Используйте следующий код подтверждения:</p>
                        <div class='code'>{verificationCode}</div>
                        <p>Код действителен в течение 15 минут. Если вы не запрашивали сброс пароля, проигнорируйте это письмо.</p>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} TechFlow. Все права защищены.</p>
                        </div>
                    </div>
                </body>
                </html>";

                SendEmail(recipientEmail, "Код восстановления пароля TechFlow", htmlBody);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке письма: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void SendEmail(string recipientEmail, string subject, string htmlBody)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(senderEmail, "TechFlow");
                mail.Subject = subject;
                mail.Body = htmlBody;
                mail.IsBodyHtml = true;
                mail.To.Add(new MailAddress(recipientEmail));

                using (SmtpClient smtp = new SmtpClient(smtpServer, smtpPort))
                {
                    smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Timeout = 200000;

                    smtp.Send(mail);
                }
            }
        }
    }
}