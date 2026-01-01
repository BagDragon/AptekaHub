using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AptekaInternetApp.ViewModel
{
    public static class SendMessageEmail
    {
       public static int SendMessage(string email)
        {
            // Настройки SMTP-сервера Mail.ru
            string smtpServer = "smtp.mail.ru"; //smpt сервер(зависит от почты отправителя)
            int smtpPort = 587; // Обычно используется порт 587 для TLS
            string smtpUsername = "aptekahubapp@mail.ru"; //твоя почта, с которой отправляется сообщение
            string smtpPassword = "AW7V5w9hYt7iWmadyHTr";//пароль приложения (от почты)

            Random rand = new Random();
           int code = rand.Next(1000, 9999);



            // Создаем объект клиента SMTP
            using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                // Настройки аутентификации
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;

                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(smtpUsername);
                    mailMessage.To.Add(email); // Укажите адрес получателя
                    mailMessage.Subject = "Восстановление пароля";
                    mailMessage.Body = $"Ваш код подтверждения: {code}";

                    try
                    {                       
                        smtpClient.Send(mailMessage);
                        MessageBox.Show("Код подтверждения отправлен на вашу почту", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        return code;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка отправки сообщения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return -1;
                    }
                }
            }
                       
        }
    }
}
