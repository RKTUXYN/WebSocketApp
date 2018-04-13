using System;
using System.Collections.Generic;

namespace SOW.Framework {
    [System.Serializable]
    public class SettingsJson {
        public string AUTHOR { get; set; }
        public string HOST_NAME { get; set; }
        public bool IS_WEB { get; set; }
        public bool IS_SECURE { get; set; }
        public bool AUTH_TYPE_BROWSER_END_SESSION { get; set; }
        public string SIGNALR_API { get; set; }
        public bool SIGNALR_IS_HOST_APPLICATION { get; set; }
        public List<string> SIGNALR_URI { get; set; }
        public string SIGNALR_ABSOLUTE_PATH { get; set; }
        public List<string> SIGNALR_ORGINS { get; set; }
        public bool DB_IS_ENCRYPT { get; set; }
        public string DB_CONNECTION { get; set; }
        public string ABSOLUTE_PATH { get; set; }
        public string EMAIL_FROM { get; set; }
        public string EMAIL_PASSWORD { get; set; }
        public string SMTP_LOGIN_ID { get; set; }
        public string SMTP_SERVER { get; set; }
        public string SMTP_PORT { get; set; }
        public string SMS_API_URI { get; set; }
        public bool EMAIL_INFO_IS_ENCRYPT { get; set; }
        public string DEFALUT_AUTH { get; set; }
        public List<string> RESPONSIBLE_EMAIL_ADDRESS { get; set; }
        public bool SEND_INFO_BY_EMAIL { get; set; }
        public List<string> RESPONSIBLE_PHONE { get; set; }
        public bool SEND_INFO_BY_SMS { get; set; }
        public string TOKEN_KEY { get; set; }
        public string AUTH_KEY { get; set; }
        public string AUTH_KEY_SUPPORT { get; set; }
        public int AUTH_EXPIRATION { get; set; }
        public string SIGNALR_AUTH_KEY { get; set; }
        public string SIGNALR_REF { get; set; }
    }
    public class Global {
        [Obsolete("Global is obsoleted for this type, please use SOW.Framework.Global instead.", true)]
        public Global( ) {
            throw new Exception("Shouldn't be here! :(");
        }
        public static string APP_DIR { get; set; }
        static bool _IS_CATCH = false;
        public static bool IS_CATCH { get { return _IS_CATCH; } }
        static SettingsJson _AppSettings;
        public static bool IS_WEB { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.IS_WEB; } }
        public static string HOST_NAME { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.HOST_NAME; } }
        public static string AUTHOR { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.AUTHOR; } }
        public static bool IS_SECURE { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.IS_SECURE; } }
        public static bool AUTH_TYPE_BROWSER_END_SESSION { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.AUTH_TYPE_BROWSER_END_SESSION; } }
        public static string SIGNALR_API { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.SIGNALR_API; } }
        public static bool SIGNALR_IS_HOST_APPLICATION { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.SIGNALR_IS_HOST_APPLICATION; } }
        public static List<string> SIGNALR_URI { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.SIGNALR_URI; } }
        public static string SIGNALR_ABSOLUTE_PATH { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.SIGNALR_ABSOLUTE_PATH; } }
        public static List<string> SIGNALR_ORGINS { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.SIGNALR_ORGINS; } }
        public static string DB_CONNECTION { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.DB_CONNECTION; } }
        public static string ABSOLUTE_PATH { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.ABSOLUTE_PATH; } }
        public static string EMAIL_FROM { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.EMAIL_FROM; } }
        public static string EMAIL_PASSWORD { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.EMAIL_PASSWORD; } }
        public static string SMTP_LOGIN_ID { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.SMTP_LOGIN_ID; } }
        public static string SMTP_SERVER { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.SMTP_SERVER; } }
        public static string SMTP_PORT { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.SMTP_PORT; } }
        public static string SMS_API_URI { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.SMS_API_URI; } }
        public static bool EMAIL_INFO_IS_ENCRYPT { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.EMAIL_INFO_IS_ENCRYPT; } }
        public static string DEFALUT_AUTH { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.DEFALUT_AUTH; } }
        public static List<string> RESPONSIBLE_EMAIL_ADDRESS { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.RESPONSIBLE_EMAIL_ADDRESS; } }
        public static bool SEND_INFO_BY_EMAIL { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.SEND_INFO_BY_EMAIL; } }
        public static List<string> RESPONSIBLE_PHONE { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.RESPONSIBLE_PHONE; } }
        public static bool SEND_INFO_BY_SMS { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.SEND_INFO_BY_SMS; } }
        public static string TOKEN_KEY { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.TOKEN_KEY; } }
        public static string AUTH_KEY { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.AUTH_KEY; } }

        public static string AUTH_KEY_SUPPORT { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.AUTH_KEY_SUPPORT; } }
        public static int AUTH_EXPIRATION { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.AUTH_EXPIRATION; } }

        public static string SIGNALR_AUTH_KEY { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.SIGNALR_AUTH_KEY; } }
        public static string SIGNALR_REF { get { if (!_IS_CATCH || _AppSettings == null) throw new Exception("Settings is not initialized yet!!! :("); return _AppSettings.SIGNALR_REF; } }
        public static void SetSettings( SettingsJson appSettings ) {
            if(string.IsNullOrEmpty(appSettings.TOKEN_KEY)
               || string.IsNullOrEmpty(appSettings.AUTH_KEY)
               || string.IsNullOrEmpty(appSettings.SIGNALR_AUTH_KEY)
               || string.IsNullOrEmpty(appSettings.SIGNALR_REF)) {
                throw new NullReferenceException("Setting(s) Property==>`TOKEN_KEY, AUTH_KEY, SIGNALR_AUTH_KEY, SIGNALR_REF` required!!!");
            }
            Global._IS_CATCH = true;
            Global._AppSettings = appSettings;
        }
    }

}
