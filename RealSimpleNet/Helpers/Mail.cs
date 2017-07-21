using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace RealSimpleNet.Helpers
{
    class Mailer
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool IsHtml { get; set; }

        private string userEmail = "";
        private string userPassword = "";
        private string server = "";
        private int port = 587;
        private bool isSecure = true;
        private List<Attachment> attachments;

        /// <summary>
        /// Creates an instance of Mail class
        /// Initialize variables
        /// </summary>
        /// <param name="server">The server address</param>
        /// <param name="userEmail">Email to authenticate</param>
        /// <param name="userPassword">User password</param>
        /// <param name="port">The port to use</param>
        /// <param name="isSecure">Indicates if must user TLS</param>
        public Mailer(string server, string userEmail, string userPassword, int port = 587, bool isSecure = true)
        {
            this.userEmail = userEmail;
            this.userPassword = userPassword;
            this.port = port;
            this.isSecure = isSecure;
            attachments = new List<Attachment>();
            IsHtml = false;
        }

        /// <summary>
        /// Adds an attachment to the list
        /// </summary>
        /// <param name="fileName"></param>
        public void AddAttachment(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new Exception("Archivo adjunto no existe");
            }

            this.attachments.Add(new Attachment(fileName));
        }

        /// <summary>
        /// Send the email
        /// </summary>
        public void Send()
        {
            MailAddress from = new MailAddress(this.FromAddress);
            MailAddress to = new MailAddress(this.ToAddress);
            SmtpClient smtp = new SmtpClient();
            smtp.Host = this.server;
            smtp.Port = this.port;
            smtp.EnableSsl = this.isSecure;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(this.userEmail, this.userPassword);

            MailMessage email = new MailMessage(this.userEmail, this.ToAddress);
            email.Subject = this.Subject;
            email.Body = this.Message;
            email.IsBodyHtml = this.IsHtml;

            foreach (Attachment attachment in this.attachments)
            {
                email.Attachments.Add(attachment);
            }

            smtp.Send(email);

        } // end void send  

        private const string MatchEmailPattern =
                  @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
           + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
           + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
           + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

        public static bool IsEmail(string email)
        {
            if (email != null) return System.Text.RegularExpressions.Regex.IsMatch(email, MatchEmailPattern);
            else return false;
        }
    } // end class Mail
} // end namespace
