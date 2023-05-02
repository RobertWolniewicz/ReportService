using Cipher;
using EmailSender;
using ReportService.Core;
using ReportService.Core.Repositories;
using ReportService.Models;
using System;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;

namespace ReportService
{
    public partial class ReportService : ServiceBase
    {
        private readonly int _sendHour;
        private readonly int _intervalInMinutes;
        private readonly bool _raportShoudBeSend;
        private Timer _timer;
        private ErrorRepository _errorRepository = new ErrorRepository();
        private ReportRepository _reportRepository = new ReportRepository();
        private static readonly NLog.Logger Logger =
            NLog.LogManager.GetCurrentClassLogger();
        private Email _email;
        private GenerateHtmlEmail _generateHtmlEmail = new GenerateHtmlEmail();
        private string _emailReceiver;
        private StringCipher _stringCipher = new StringCipher("E403B04B-45F8-4580-AD8F-072A7BDDFC01");
        private const string NonEncryptedPasswordPrefix = "encrypt:";

        public ReportService()
        {
            InitializeComponent();


            try
            {
                _sendHour = Convert.ToInt32(ConfigurationManager.AppSettings["SendHours"]);
                _intervalInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalInMinutes"]);
                _raportShoudBeSend = Convert.ToBoolean(ConfigurationManager.AppSettings["RaportShoudBeSend"]);
                _timer = new Timer(_intervalInMinutes * 60000);

                _emailReceiver = ConfigurationManager.AppSettings["ReceiverEmail"];

                _email = new Email(new EmailParams
                {
                    HostSmtp = ConfigurationManager.AppSettings["HostSmtp"],
                    Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]),
                    EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]),
                    SenderName = ConfigurationManager.AppSettings["SenderName"],
                    SenderEmail = ConfigurationManager.AppSettings["SenderEmail"],
                    SenderEmailPassword = DecryptSenderEmailPassword()
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private string DecryptSenderEmailPassword()
        {
            var encryptedPassword = ConfigurationManager.AppSettings
                ["SenderEmailPasswor"];

            if (encryptedPassword.StartsWith(NonEncryptedPasswordPrefix))
            {
                encryptedPassword = _stringCipher
                    .Encrypt(encryptedPassword.Replace(NonEncryptedPasswordPrefix, ""));
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configFile.AppSettings.Settings["SenderEmailPasswor"].Value = encryptedPassword;

                configFile.Save();
            }

            return _stringCipher.Decrypt(encryptedPassword);
        }

        protected override void OnStart(string[] args)
        {
            _timer.Elapsed += DoWork;
            _timer.Start();
            Logger.Info("Service started.");
        }

        private async void DoWork(object sender, ElapsedEventArgs e)
        {
            try
            {
                await SendError();
                await SendReport();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw new Exception(ex.Message);
            }


        }

        private async Task SendReport()
        {
            if (!_raportShoudBeSend)
                return;

            var actualHour = DateTime.Now.Hour;

            if (actualHour < _sendHour)
            {
                return;
            }

            var report = _reportRepository.GetLastNotSentReport();

            if (report == null)
            {
                return;
            }

            await _email.Send("Raport dobowy", _generateHtmlEmail.GenerateReport(report),
                _emailReceiver);

            _reportRepository.ReportSent(report);

            Logger.Info("Report sent.");

        }

        private async Task SendError()
        {
            var errors = _errorRepository.GetLastErrors(_intervalInMinutes);

            if (errors == null || !errors.Any())
                return;

            await _email.Send("Błędy w aplikacji", _generateHtmlEmail.GenerateErrors(errors,
                _intervalInMinutes), _emailReceiver);

            Logger.Info("Error sent.");
        }

        protected override void OnStop()
        {
            Logger.Info("Service stoped...");
        }
    }
}
