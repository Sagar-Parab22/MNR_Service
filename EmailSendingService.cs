using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EmailService
{

    public partial class EmailSendingService : ServiceBase
    {
        private System.Timers.Timer _timer = null;

        public EmailSendingService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _timer = new System.Timers.Timer();
            _timer.Interval = Convert.ToDouble(ConfigurationManager.AppSettings["TimerIntervalMilliseconds"]);
            _timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
            _timer.Enabled = true;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Enabled = false; // Stop timer while we work
            try
            {
                // 1. Fetch the list of emails that need to be sent
                List<dputEmailStructure> emails = FetchEmailsToSend();

                // 2. Loop through and send each email
                foreach (var email in emails)
                {
                    try
                    {
                        SendEmail(email);
                        string successRemark = $"Email sent successfully at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                        WriteLog(successRemark);

                        // Save the success remark to the database.
                        UpdateEmailStatus(email.ID, "Sent", successRemark);
                    }
                    catch (Exception ex)
                    {
                        string failureRemark = $"FAILED to send email to {email.To}. Error: {ex.Message}";
                        WriteLog(failureRemark);
                        UpdateEmailStatus(email.ID, "Failed", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog($"An error occurred in the main processing job: {ex.Message}");
            }
            finally
            {
                _timer.Enabled = true; // Restart timer
                WriteLog("Email processing job finished. Waiting for next interval.");
            }
        }

        private string GetQueryFromFile(string fileName)
        {
            string folderPath = ConfigurationManager.AppSettings["QueryFolderPath"];
            // Combine the base directory of the service with the relative folder path and file name
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderPath, fileName);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"The query file could not be found: {fullPath}");
            }

            return File.ReadAllText(fullPath);
        }

        protected override void OnStop()
        {
            _timer.Enabled = false;
            WriteLog("EmailSendingService has stopped.");
        }

        private List<dputEmailStructure> FetchEmailsToSend()
        {
            var emailList = new List<dputEmailStructure>();
            string connStr = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
            try
            {
                var commTypes = (System.Collections.Specialized.NameValueCollection)ConfigurationManager.GetSection("communicationTypes");
                string commTypeName = ConfigurationManager.AppSettings["ProcessingCommunicationType"];


                string commTypeIdString = commTypes[commTypeName];
                int idToQuery;
                if (string.IsNullOrEmpty(commTypeIdString) || !int.TryParse(commTypeIdString, out idToQuery))
                {
                    WriteLog($"Invalid ProcessingCommunicationType in App.config: '{commTypeName}' is not defined in the <communicationTypes> section.");
                    return emailList; 
                }

                string query = GetQueryFromFile("FetchPendingEmails.sql");

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CommunicationTypeID", idToQuery);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                emailList.Add(new dputEmailStructure
                                {
                                    ID = Convert.ToInt32(reader["ID"]),
                                    To = reader["Receiver"].ToString(),
                                    Cc = reader["ReceiverCC"].ToString(),
                                    Bcc = reader["ReceiverBCC"].ToString(),
                                    Subject = reader["Subject"].ToString(),
                                    Body = reader["Body"].ToString(),
                                    FilePath = reader["FilePath"].ToString()
                                });
                            }
                        } // The reader is guaranteed to be closed here!
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog($"DATABASE ERROR in FetchEmailsToSend: {ex.Message}");
            }
            return emailList;
        }

        private void SendEmail(dputEmailStructure email)
        {
            // Read settings from App.config
            string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
            bool enableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);
            string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
            string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = enableSsl;
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(smtpUser);
                    mailMessage.To.Add(email.To);
                    if (!string.IsNullOrEmpty(email.Cc))
                    {
                        string[] ccEmails = email.Cc.Split(',');
                        // 3. Loop through each email address in the array.
                        foreach (string ccEmail in ccEmails)
                        {
                            // 4. Trim whitespace and add the address to the CC list.
                            if (!string.IsNullOrWhiteSpace(ccEmail))
                            {
                                mailMessage.CC.Add(ccEmail.Trim());
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(email.Bcc))
                    {
                        string[] bccEmails = email.Bcc.Split(',');
                        foreach (string bccEmail in bccEmails)
                        {
                            if (!string.IsNullOrWhiteSpace(bccEmail))
                            {
                                mailMessage.Bcc.Add(bccEmail.Trim());
                            }
                        }
                    }

                    mailMessage.Subject = email.Subject;
                    mailMessage.Body = email.Body;
                    mailMessage.IsBodyHtml = true;

                    if (!string.IsNullOrEmpty(email.FilePath) && File.Exists(email.FilePath))
                    {
                        // If a path is given, the file MUST exist.
                        if (File.Exists(email.FilePath))
                        {
                            mailMessage.Attachments.Add(new Attachment(email.FilePath));
                        }
                        else
                        {
                            // If the file does not exist, throw a specific exception.
                            // This will be caught in OnTimerElapsed and logged to the database.
                            throw new FileNotFoundException($"Attachment file not found: {email.FilePath}");
                        }
                    }

                    client.Send(mailMessage);
                }
            }
        }

        private void UpdateEmailStatus(int emailId, string status, string reason)
        {
            string connStr = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

            string query = GetQueryFromFile("UpdateEmailStatus.sql");

            // We'll translate our "Sent" status to the '1' used in your old table.
            int dbStatus = (status == "Sent") ? 1 : 0;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", dbStatus);
                    cmd.Parameters.AddWithValue("@Reason", (object)reason ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SendDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@EmailId", emailId);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"DATABASE ERROR in UpdateEmailStatus: {ex.Message}");
                    }
                }
            }
        }

        private void WriteLog(string logMessage)
        {
            try
            {
                string logPath = ConfigurationManager.AppSettings["LogFilePath"];
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                using (StreamWriter writer = new StreamWriter(logPath, true))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {logMessage}");
                }
            }
            catch
            {
                // If logging fails, do nothing to prevent the service from crashing.
            }
        }
        public void StartService()
        {
            this.OnStart(null);
        }

        public void StopService()
        {
            this.OnStop();
        }
    }

}
