using ArtemisBanking.Core.Application.Dtos.Email;
using ArtemisBanking.Core.Application.Enums;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ArtemisBanking.Infrastructure.Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailRequestDto emailRequestDto)
        {
            try
            {
                emailRequestDto.ToRange?.Add(emailRequestDto.To ?? "");

                MimeMessage email = new()
                {
                    
                    Sender = MailboxAddress.Parse(_mailSettings.EmailFrom),
                    Subject = emailRequestDto.Subject
                };

                foreach (var toItem in emailRequestDto.ToRange ?? [])
                {
                    email.To.Add(MailboxAddress.Parse(toItem));
                }

                BodyBuilder builder = new()
                {
                    HtmlBody = emailRequestDto.HtmlBody
                };
                email.Body = builder.ToMessageBody();

                using MailKit.Net.Smtp.SmtpClient smtpClient = new();
                await smtpClient.ConnectAsync(_mailSettings.SmtpHost, _mailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occured {Exception}.", ex);
            }
        }

        public async Task SendTemplateEmailAsync(EmailTemplateDataDto data)
        {
            string templatePath = GetTemplatePath(data.Type);
            string html = await File.ReadAllTextAsync(templatePath);
            string subject = GetSubjectFor(data.Type);

            foreach (var variable in data.Variables)
            {
                html = html.Replace($"{{{{{variable.Key}}}}}", variable.Value);
                subject = subject.Replace($"{{{{{variable.Key}}}}}", variable.Value);
            }

            await SendAsync(new EmailRequestDto
            {
                To = data.To,
                Subject = subject,
                HtmlBody = html
            });
        }

        private string GetTemplatePath(EmailType type)
        {
            // Ruta base del ejecutable final
            string baseDir = AppContext.BaseDirectory;

            // Carpeta Templates dentro de bin/...
            string templatesDir = Path.Combine(baseDir, "Templates");

            return type switch
            {
                EmailType.LoanApproved =>
                    Path.Combine(templatesDir, "LoanApproved.html"),

                EmailType.LoanRateUpdated =>
                    Path.Combine(templatesDir, "LoanRateUpdated.html"),

                EmailType.LoanDisbursement =>
                    Path.Combine(templatesDir, "LoanDisbursement.html"),

                EmailType.LoanPayment =>
                    Path.Combine(templatesDir, "LoanPayment.html"),

                EmailType.CreditCardPayment =>
                    Path.Combine(templatesDir, "CreditCardPayment.html"),

                EmailType.CashAdvance =>
                    Path.Combine(templatesDir, "CashAdvance.html"),
                
                EmailType.CreditCardLimitUpdated =>
                    Path.Combine(templatesDir, "CreditCardLimitUpdated.html"),               

                EmailType.ExpressTransferSender =>
                    Path.Combine(templatesDir, "ExpressTransferSender.html"),

                EmailType.ExpressTransferReceiver =>
                    Path.Combine(templatesDir, "ExpressTransferReceiver.html"),

                EmailType.BeneficiaryTransferSender =>
                    Path.Combine(templatesDir, "BeneficiaryTransferSender.html"),

                EmailType.BeneficiaryTransferReceiver =>
                    Path.Combine(templatesDir, "BeneficiaryTransferReceiver.html"),
                
                EmailType.Deposit =>
                    Path.Combine(templatesDir, "Deposit.html"),

                EmailType.Withdraw =>
                    Path.Combine(templatesDir, "Withdraw.html"),

                _ => throw new Exception($"No existe plantilla para {type}")
            };
        }

        
        
        private string GetSubjectFor(EmailType type)
        {
            return type switch
            {

                EmailType.LoanApproved => "Tu préstamo ha sido aprobado",
                EmailType.LoanRateUpdated => "Actualización de tasa de tu préstamo",
                EmailType.LoanDisbursement => "Desembolso de préstamo realizado",
                EmailType.LoanPayment => "Pago realizado al préstamo {{LoanNumber}}",

                EmailType.CreditCardPayment => "Pago realizado a la tarjeta {{CardLast4}}",
                EmailType.CashAdvance => "Avance de efectivo realizado",
                EmailType.CreditCardLimitUpdated => "Actualización de límite de su tarjeta de crédito",

                EmailType.ExpressTransferSender => "Transacción realizada a la cuenta {{DestAccountLast4}}",
                EmailType.ExpressTransferReceiver => "Transacción enviada desde la cuenta {{OriginAccountLast4}}",

                EmailType.BeneficiaryTransferSender => "Transferencia enviada a beneficiario",
                EmailType.BeneficiaryTransferReceiver => "Has recibido una transferencia de beneficiario",
                
                EmailType.Deposit => "Depósito realizado a su cuenta {{AccountLast4}}",
                EmailType.Withdraw => "Retiro realizado de su cuenta {{AccountLast4}}",

                _ => "Notificación – ArtemisBanking"
            };
        }


    }
}
