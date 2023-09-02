using System;
using System.IO;
using System.Net.Mail;
using System.Text;
using FolderDog.Interfaces;
using FolderDog.Models;
using FolderDog.Options;
using Serilog;

namespace FolderDog.Services
{
    /// <summary>
    /// <see cref="IMessageSender"/>
    /// </summary>
    public class EmailService : IMessageSender
    {      
        private readonly EmailOptions _emailOptions;
        private readonly ILogger _logger;
        public EmailService(EmailOptions options, ILogger logger)
        {
            _emailOptions = options;
            _logger = logger;
        }

        /// <summary>
        /// <see cref="IMessageSender.SendMessage(string, string?)"/>
        /// </summary>
        public Result SendMessage(string message, FileStream fileStream = null, string fileName = null)
        {
            var result = new Result();
            using var mailMessage = new MailMessage();
            using var smtpClient = new SmtpClient(
                _emailOptions.SmtpServerIpv4Address,
                _emailOptions.SmtpServerPortNumber);
            
            try
            {
                MessageBuilder(mailMessage, message, fileStream, fileName);
            }
            catch (IOException ex)
            {
                result.Errors.Add(ex.Message);
                _logger.Error("Unable to generate email due to exception '{Exception}'", ex.Message);
                return result;
            }

            try
            {
                string sendTos = string.Join(",", _emailOptions.SendTos);
                _logger.Information("Sending message to '{Recipients}' with the message subject '{Subject}' and attachment '{Attachment}'...",
                    sendTos, 
                    _emailOptions.MessageSubject,
                    fileName ?? string.Empty);

                smtpClient.Send(mailMessage);
                _logger.Information("Message has been sent.");

                result.Message = $"Message has been sent to '{mailMessage.To}'";     
            }
            catch(SmtpException ex)
            {
                result.Errors.Add(ex.Message);
                _logger.Error(ex.Message);
                if (ex?.InnerException?.Message is not null)
                {
                    _logger.Warning("Inner error '{InnerError}'", ex.InnerException.Message);
                }
            }

            return result;
        }

        /// <summary>
        /// Generate message
        /// </summary>
        /// <param name="mailMessage">Mail Message to build</param>
        /// <param name="message">Message body</param>
        /// <param name="pathToAttachment"></param>
        /// <returns><see cref="MailMessage"/>attachment</returns>
        private MailMessage MessageBuilder(
            MailMessage mailMessage,
            string message,
            FileStream fileStream = null,
            string fileName = null)
        {
            mailMessage.From = new MailAddress(_emailOptions.SendFrom);
            
            foreach (var sendTo in _emailOptions.SendTos)
            {
                mailMessage.To.Add(new MailAddress(sendTo));
            }

            foreach (var sendBcc in _emailOptions.SendBccs)
            {
                mailMessage.Bcc.Add(new MailAddress(sendBcc));
            }

            foreach (var sendCc in _emailOptions.SendCcs)
            {
                mailMessage.CC.Add(new MailAddress(sendCc));
            }

            if (fileStream is not null)
            {
                mailMessage.Attachments.Add(new Attachment(fileStream, fileStream.Name));
            }
            mailMessage.Body = message;
            mailMessage.Subject = _emailOptions.MessageSubject;

            return mailMessage;
        }
    }
}