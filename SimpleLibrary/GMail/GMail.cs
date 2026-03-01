using Autofac;
using SimpleLibrary.Logger;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleLibrary.GMail
{
    public class GMail : PrintLogger
    {
        private readonly string _EmailAddress = "";
        private readonly string _EmailPassword = "";
        private string _smtpHost = "smtp.gmail.com";
        private int _smtpPort = 587;

        public GMail(string emailAddress, string emailPassword, ContainerBuilder builder = null)
        {
            _EmailAddress = emailAddress;
            _EmailPassword = emailPassword;
            InitLogger(builder);
        }

        public GMail(string emailAddress, string emailPassword, string smtpHost, int smtpPort, ContainerBuilder builder = null)
        {
            _EmailAddress = emailAddress;
            _EmailPassword = emailPassword;
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            InitLogger(builder);
        }

        public void SendMessage(string displayName, string subject, string body, List<string> ToAdd)
        {
            SendMessage(displayName, subject, body, ToAdd, null, null);
        }

        public void SendMessage(string displayName, string subject, string body, List<string> ToAdd, 
                                 List<string> attachments, Dictionary<string, string> inlineImages)
        {
            using (MailMessage mailMessage_ = new MailMessage())
            {
                mailMessage_.From = new MailAddress(_EmailAddress, displayName);

                for (int i = 0; i < ToAdd.Count; ++i)
                {
                    mailMessage_.To.Add(ToAdd[i]);
                }

                if (mailMessage_.To.Count > 0)
                {
                    mailMessage_.Priority = MailPriority.Normal;
                    mailMessage_.Subject = subject;
                    mailMessage_.IsBodyHtml = true;

                    if (inlineImages != null && inlineImages.Count > 0)
                    {
                        var htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                        foreach (var kvp in inlineImages)
                        {
                            LinkedResource linkedImage = new LinkedResource(kvp.Key)
                            {
                                ContentId = kvp.Value
                            };
                            htmlView.LinkedResources.Add(linkedImage);
                        }
                        mailMessage_.AlternateViews.Add(htmlView);
                        mailMessage_.Body = body;
                    }
                    else
                    {
                        mailMessage_.Body = body;
                    }

                    if (attachments != null)
                    {
                        foreach (var attachmentPath in attachments)
                        {
                            if (System.IO.File.Exists(attachmentPath))
                            {
                                Attachment attachment = new Attachment(attachmentPath);
                                mailMessage_.Attachments.Add(attachment);
                            }
                        }
                    }

                    using (SmtpClient smtpClient_ = new SmtpClient(_smtpHost, _smtpPort)
                    {
                        Credentials = new NetworkCredential(_EmailAddress, _EmailPassword),
                        EnableSsl = true
                    })
                    {
                        smtpClient_.Send(mailMessage_);
                    }
                }
            }
        }

        public async Task SendMessageAsync(string displayName, string subject, string body, List<string> ToAdd, 
                                           CancellationToken cancellationToken = default)
        {
            await SendMessageAsync(displayName, subject, body, ToAdd, null, null, cancellationToken);
        }

        public async Task SendMessageAsync(string displayName, string subject, string body, List<string> ToAdd,
                                           List<string> attachments, Dictionary<string, string> inlineImages,
                                           CancellationToken cancellationToken = default)
        {
            using (MailMessage mailMessage_ = new MailMessage())
            {
                mailMessage_.From = new MailAddress(_EmailAddress, displayName);

                for (int i = 0; i < ToAdd.Count; ++i)
                {
                    mailMessage_.To.Add(ToAdd[i]);
                }

                if (mailMessage_.To.Count > 0)
                {
                    mailMessage_.Priority = MailPriority.Normal;
                    mailMessage_.Subject = subject;
                    mailMessage_.IsBodyHtml = true;

                    if (inlineImages != null && inlineImages.Count > 0)
                    {
                        var htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                        foreach (var kvp in inlineImages)
                        {
                            LinkedResource linkedImage = new LinkedResource(kvp.Key)
                            {
                                ContentId = kvp.Value
                            };
                            htmlView.LinkedResources.Add(linkedImage);
                        }
                        mailMessage_.AlternateViews.Add(htmlView);
                        mailMessage_.Body = body;
                    }
                    else
                    {
                        mailMessage_.Body = body;
                    }

                    if (attachments != null)
                    {
                        foreach (var attachmentPath in attachments)
                        {
                            if (System.IO.File.Exists(attachmentPath))
                            {
                                Attachment attachment = new Attachment(attachmentPath);
                                mailMessage_.Attachments.Add(attachment);
                            }
                        }
                    }

                    using (SmtpClient smtpClient_ = new SmtpClient(_smtpHost, _smtpPort)
                    {
                        Credentials = new NetworkCredential(_EmailAddress, _EmailPassword),
                        EnableSsl = true
                    })
                    {
                        await smtpClient_.SendMailAsync(mailMessage_, cancellationToken);
                    }
                }
            }
        }
    }
}
