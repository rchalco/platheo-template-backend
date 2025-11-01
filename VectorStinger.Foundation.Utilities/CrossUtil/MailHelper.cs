using PlumbingProps.Document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Foundation.Utilities.CrossUtil
{
    public class MailHelper
    {
        public static string smtServer { get; set; }
        public static string smtUser { get; set; }
        public static string smtPassword { get; set; }
        public static string smtPort { get; set; }

        public enum Type
        {
            Alerta,
            Error,
            Notificacion
        }

        /// <summary>
        /// Envia un mail por STMP
        /// </summary>
        /// <param name="From">direccion de remitente</param>
        /// <param name="To">direccion del destinatario</param>
        /// <param name="subject">Asunto</param>
        /// <param name="mensaje">Mensaje</param>
        /// <param name="isHtml">indica si el mail es HTML o no</param>
        public static void SendMail(MailAddress From, List<MailAddress> To, string subject, string mensaje, bool isHtml = false)
        {
            MailMessage msg = new MailMessage();

            foreach (var item in To)
            {
                msg.To.Add(item);
            }
            msg.From = From;
            msg.Subject = subject;
            msg.Body = mensaje;
            msg.Priority = MailPriority.High;
            msg.IsBodyHtml = isHtml;

            SmtpClient smtp = new SmtpClient(smtServer);
            smtp.Port = Convert.ToInt32(smtPort);
            smtp.Credentials = new System.Net.NetworkCredential(smtUser, smtPassword);
            smtp.EnableSsl = true;
            smtp.Send(msg);
        }


        /// <summary>
        /// Envia un mail por STMP
        /// </summary>
        /// <param name="From">direccion de remitente</param>
        /// <param name="To">direccion del destinatario</param>
        /// <param name="subject">Asunto</param>
        /// <param name="mensaje">Mensaje</param>
        /// <param name="isHtml">indica si el mail es HTML o no</param>
        public static void SendMail(MailAddress From, List<MailAddress> To, string subject, string mensaje, bool isHtml = false, byte[] fileAttach = null, string nameFileAttach = "")
        {
            MailMessage msg = new MailMessage();

            foreach (var item in To)
            {
                msg.To.Add(item);
            }
            msg.From = From;
            msg.Subject = subject;
            msg.Body = mensaje;
            msg.Priority = MailPriority.High;
            msg.IsBodyHtml = isHtml;

            if (fileAttach != null)
            {
                System.Net.Mime.ContentType contentType = new System.Net.Mime.ContentType();
                contentType.MediaType = System.Net.Mime.MediaTypeNames.Application.Octet;
                contentType.Name = nameFileAttach;
                Stream stream = new MemoryStream(fileAttach);
                msg.Attachments.Add(new Attachment(stream, contentType));
            }

            SmtpClient smtp = new SmtpClient(smtServer);
            smtp.Port = Convert.ToInt32(smtPort);
            smtp.Credentials = new System.Net.NetworkCredential(smtUser, smtPassword);
            smtp.EnableSsl = true;
            smtp.Send(msg);
        }


        /// <summary>
        /// Envia un mail por STMP
        /// </summary>
        /// <param name="From">direccion de remitente</param>
        /// <param name="To">direccion del destinatario</param>
        /// <param name="subject">Asunto</param>
        /// <param name="mensaje">Mensaje</param>
        /// <param name="isHtml">indica si el mail es HTML o no</param>
        public static void SendMail(MailAddress From, List<MailAddress> To, string subject, string mensaje, bool isHtml = false, Dictionary<string, byte[]> fileToAttach = null)
        {
            MailMessage msg = new MailMessage();

            foreach (var item in To)
            {
                msg.To.Add(item);
            }
            msg.From = From;
            msg.Subject = subject + "-" + DateTime.Now.ToString();
            msg.Body = mensaje;
            msg.Priority = MailPriority.High;
            msg.IsBodyHtml = isHtml;

            fileToAttach?.Keys.ToList().ForEach(item =>
            {
                System.Net.Mime.ContentType contentType = new System.Net.Mime.ContentType();
                contentType.MediaType = System.Net.Mime.MediaTypeNames.Application.Octet;
                contentType.Name = item;
                Stream stream = new MemoryStream(fileToAttach[item]);
                msg.Attachments.Add(new Attachment(stream, contentType));
            });

            SmtpClient smtp = new SmtpClient(smtServer);
            smtp.Port = Convert.ToInt32(smtPort);
            smtp.Credentials = new System.Net.NetworkCredential(smtUser, smtPassword);
            smtp.EnableSsl = true;
            smtp.Send(msg);
        }


    }
}
