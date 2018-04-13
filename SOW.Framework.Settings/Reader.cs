using System;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;

namespace SOW.Framework.Settings {
    using SOW.Framework.PgSQL.Database;
    using SOW.Framework.Security;
    using SOW.Framework.Files;
    using SOW.Framework.Log;
    public class Helper {
        static string _HostName;
        internal static string HostName {
            get {
                return _HostName;
            }
        }
        internal class FirstRequestInitialization {
            private static string host = null;
            private static Object s_lock = new Object();
            public static string Initialize( HttpContext context, bool real = true ) {
                if (string.IsNullOrEmpty(host)) {
                    lock (s_lock) {
                        var uri = context.Request.Url;
                        host = uri.GetLeftPart(UriPartial.Authority);
                        if (real) {
                            string[] arr = host.Split(new[] { "://" }, StringSplitOptions.None);
                            if (arr == null) {
                                host = "INVALID";
                                return host;
                            }
                            host = arr[1];
                        }
                    }
                }
                if (!string.IsNullOrEmpty(host)) {
                    host = host.ToLower();
                    if (host == "localhost") host = null;//localhost is not allowed!!!
                    _HostName = host;
                }
                return host;
            }
        }
        public static void UrlForwardToLower( HttpContext context ) {
            if (context.Request.HttpMethod.ToUpper() != "GET") return;
            string lowercaseURL = (context.Request.Url.Scheme + "://" + context.Request.Url.Authority + context.Request.Url.AbsolutePath);
            if (!System.Text.RegularExpressions.Regex.IsMatch(lowercaseURL, @"[A-Z]")) {
                return;
            }
            context.Response.RedirectPermanent(
                     lowercaseURL.ToLower() + context.Request.Url.Query
            );
        }
    }
    public class Reader {
        static bool _IS_CATCH = false;
        public static bool IS_CATCH { get { return _IS_CATCH; } }
        public static void SetAppDir( string appDir ) {
            Global.APP_DIR = appDir;
        }
        /// <summary>
        ///  Prepare the Host management settings json.
        ///  Returns:
        ///    An System.Boolean that contains true or false.
        ///  Exceptions:
        ///   System.IO.FileNotFoundException:
        ///     No settings file found.
        /// </summary>
        public static bool Initialize( bool isHostApplication = false, bool _throw = false ) {
            if (_IS_CATCH) return true;
            if (isHostApplication) {
                var _assembly = System.Reflection.Assembly.GetEntryAssembly().Location;
                Global.APP_DIR = System.IO.Path.GetDirectoryName(_assembly);
            }
            var _AppSettings = new SettingsJson();
            string settingsDir = string.Format(@"{0}/settings/", Global.APP_DIR);
            DirectoryWorker.CreateDirectory(settingsDir);
            string logDir = string.Format(@"{0}/", Global.APP_DIR);
            Log.LOG_DIR = logDir;
            DirectoryWorker.CreateDirectory(logDir);
            string filePath = string.Format(@"{0}settings.json", settingsDir);
            string msg = "";
            if (!File.Exists(filePath)) {
                msg = string.Format("No Setting(s) file found in following path {0};", filePath);
                Log.WriteAsync(msg, true);
                
                if (_throw)
                    throw new Exception("Invalid settings. Contact with web administrator... :(");
                return false;
            }
            string infoJson = FileWorker.Read(filePath);
            if (string.IsNullOrEmpty(infoJson)) {
                msg = string.Format("No Setting(s) Data found in following path {0};", filePath);
                Log.WriteAsync(msg, true);
                if (_throw)
                    throw new Exception("Invalid settings. Contact with web administrator... :(");
                return false;
            }
            _AppSettings = new JavaScriptSerializer().Deserialize<SettingsJson>(infoJson); infoJson = null;
            if (string.IsNullOrEmpty(_AppSettings.DB_CONNECTION)) {
                msg = string.Format("No Database Connection found in following path {0};", filePath);
                Log.WriteAsync(msg, true);
                if (_throw)
                    throw new Exception("Invalid settings. Contact with web administrator... :(");
                return false;
            }
            if (_AppSettings.DB_IS_ENCRYPT) {
                _AppSettings.DB_CONNECTION = Decrypt.Token(_AppSettings.DB_CONNECTION);
            }
            Connection.Set(_AppSettings.DB_CONNECTION);
            _IS_CATCH = true; _AppSettings.SIGNALR_IS_HOST_APPLICATION = isHostApplication;
            if (_AppSettings.EMAIL_INFO_IS_ENCRYPT) {
                _AppSettings.EMAIL_FROM = Decrypt.Token(_AppSettings.EMAIL_FROM);
                _AppSettings.EMAIL_PASSWORD = Decrypt.Token(_AppSettings.EMAIL_PASSWORD);
                _AppSettings.SMTP_LOGIN_ID = Decrypt.Token(_AppSettings.SMTP_LOGIN_ID);
                _AppSettings.SMTP_SERVER = Decrypt.Token(_AppSettings.SMTP_SERVER);
            }
            if (_AppSettings.IS_WEB) {
                if (string.IsNullOrEmpty(_AppSettings.HOST_NAME))
                    _AppSettings.HOST_NAME = Helper.HostName;
            }
            Global.SetSettings(_AppSettings); _AppSettings = null;

            return true;
        }
        public static void RegisterApplicationSettings( Func<string, HttpContext> contextCb, bool _throw = false ) {
            if (_IS_CATCH) return;
            HttpContext context = contextCb("GET_CONTEXT");
            if (Helper.HostName == null)
                Helper.FirstRequestInitialization.Initialize(context, true);
            Global.APP_DIR = context.Server.MapPath("/info/");
            Reader.Initialize(false, _throw);
            return;
        }
    }
}
