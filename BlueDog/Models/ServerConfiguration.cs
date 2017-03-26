using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Tracing;

namespace BlueDog.Models
{


    public class ServerConfiguration
    {
        public const string DOCUMENTDB_ENDPOINT_URI_KEY = "BlueDogEndPointUri";
        public const string DOCUMENTDB_MASTER_KEY = "BlueDogMasterKey";

        public const string USERS_DATABASE_NAME = "UsersDatabaseName";
        public const string USERS_COLLECTION = "UsersCollection";

        private const string EMAIL_FROM_KEY = "EmailFrom";

        private const string EMAIL_SMTP_SERVER = "SmtpServer";
        private const string EMAIL_SMTP_PORT = "SmtpPort";

        private const string SMTP_USER = "SmtpUser";
        private const string SMTP_PASSWORD = "SmtpPassword";

        private const string JWT_SECRET_KEY = "JwtSecretKey";
        public const string PASSWORD_RESET_DURATION_MIN = "PasswordResetDurationMinutes";
        public const string EMAIL_RESET_EXPIRATION_MIN = "EmailResetExpirationMinutes";



        public string DocumentDBEndpointUri { get; set; }
        public string DocumentDBMasterKey { get; set; }
        public string UsersDatabaseName { get; set; }
        public string UsersCollectionName { get; set; }

        public string JwtSecretKey { get; set; }

        public string EmailValidationFrom { get; set; }
        public string EmailSmtpServer { get; set; }
        public string EmailSmtpPort { get; set; }
        public int PasswordResetDurationMinutes { get; set; }
        public int EmailResetExpirationMinutes { get; set; }
        public string SmtpUser { get; internal set; }
        public string SmtpPassword { get; internal set; }

        public static ServerConfiguration FromAppSettings(TraceWriter log)
        {
            ServerConfiguration config = new ServerConfiguration();

            config.DocumentDBEndpointUri = ConfigSetting(DOCUMENTDB_ENDPOINT_URI_KEY, log);
            config.DocumentDBMasterKey = ConfigSetting(DOCUMENTDB_MASTER_KEY, log);

            config.UsersDatabaseName = ConfigSetting(USERS_DATABASE_NAME, log);
            config.UsersCollectionName = ConfigSetting(USERS_COLLECTION, log);

            config.EmailValidationFrom = ConfigSetting(EMAIL_FROM_KEY, log);
            config.EmailSmtpServer = ConfigSetting(EMAIL_SMTP_SERVER, log);
            config.EmailSmtpPort = ConfigSetting(EMAIL_SMTP_PORT, log);

            config.JwtSecretKey = ConfigSetting(JWT_SECRET_KEY, log);
            config.PasswordResetDurationMinutes = int.Parse(ConfigSetting(PASSWORD_RESET_DURATION_MIN, log));
            config.EmailResetExpirationMinutes = int.Parse(ConfigSetting(EMAIL_RESET_EXPIRATION_MIN, log));

            config.SmtpUser = ConfigSetting(SMTP_USER, log);
            config.SmtpPassword = ConfigSetting(SMTP_PASSWORD, log);

            return config;
        }

        public static string ConfigSetting(string name, TraceWriter log)
        {
            if ( ConfigurationManager.AppSettings.AllKeys.Contains(name) == false )
            {
                log.Error($"AppSettings doesn't contain an entry for {name}");
            }
            return ConfigurationManager.AppSettings[name];
        }
    }
}
