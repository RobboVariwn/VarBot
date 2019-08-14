using System.Collections.Generic;
using NLog;
using NLogger;
using SmartLocalization;
using UnityEngine;
using Varwin.UI;
using Varwin.UI.VRErrorManager;

namespace Varwin.Errors
{
    public static class ErrorHelper
    {
        private static Dictionary<int, string> _errCodesToLocStrings = new Dictionary<int, string>();

        static ErrorHelper()
        {
            _errCodesToLocStrings[ErrorCode.CompileCodeError] = "COMPILE_CODE_ERROR";
            _errCodesToLocStrings[ErrorCode.RuntimeCodeError] = "RUNTIME_CODE_ERROR";
            _errCodesToLocStrings[ErrorCode.ReadStartupArgsError] = "ERROR_READ_STARTUP_ARGS";
            _errCodesToLocStrings[ErrorCode.ServerNoConnectionError] = "ERROR_SERVER_DISCONECT";
            _errCodesToLocStrings[ErrorCode.LogicExecuteError] = "EXCECUTE_LOGIC_ERROR";
            _errCodesToLocStrings[ErrorCode.LogicInitError] = "INIT_LOGIC_ERROR";
            _errCodesToLocStrings[ErrorCode.LicenseKeyError] = "LICENSE_KEY_ERROR";
            _errCodesToLocStrings[ErrorCode.LoadObjectError] = "LOAD_OBJECT_ERROR";
            _errCodesToLocStrings[ErrorCode.LoadSceneError] = "LOAD_SCENE_ERROR";
            _errCodesToLocStrings[ErrorCode.RabbitNoArgsError] = "READ_LAUNCH_ARGS_RABBIR_ERROR";
            _errCodesToLocStrings[ErrorCode.LocationSaveError] = "SAVE_ERROR";
            _errCodesToLocStrings[ErrorCode.SpawnPointNotFoundError] = "SPAWN_POINT_ERROR";
            _errCodesToLocStrings[ErrorCode.UnknownError] = "UNKNOWN_ERROR";
            _errCodesToLocStrings[ErrorCode.LoadWorldConfigError] = "WORLD_CONFIG_ERROR";
            _errCodesToLocStrings[ErrorCode.ProjectConfigNullError] = "WORLD_CONFIG_NULL";
            _errCodesToLocStrings[ErrorCode.ExceptionInObject] = "EXCEPTION_IN_OBJECT";
            _errCodesToLocStrings[ErrorCode.CannotPreview] = "CANNOT_NOT_PREVEW";
            _errCodesToLocStrings[ErrorCode.CannotDeleteObjectLogic] = "CANNOT_DELETE_OBJECT_LOGIC";
            _errCodesToLocStrings[ErrorCode.NotForCommercialUse] = "NOT_FOR_COMMERCIAL_USE";
        }

        public static string GetErrorKeyByCode(int errorCode) => _errCodesToLocStrings[errorCode];
        
        public static string GetErrorDescByCode(int errorCode) =>
            LanguageManager.Instance.GetTextValue(_errCodesToLocStrings[errorCode]);

        public static void DisplayErrorByCode(int errorCode)
        {
            string errorMessage =
                $"Error {errorCode}.\n {LanguageManager.Instance.GetTextValue(_errCodesToLocStrings[errorCode])}";

            VRErrorManager vrErrorManager = VRErrorManager.Instance;

            if (vrErrorManager != null)
            {
                vrErrorManager.ShowFatal(errorMessage);
            }
            
            LauncherErrorManager launcherErrorManager = LauncherErrorManager.Instance;

            if (launcherErrorManager != null)
            {
                launcherErrorManager.ShowFatal(errorMessage, null);
            }
        }


        public static void ErrorHandler(string condition, string stackTrace, LogType logType)
        {
            if (logType != LogType.Exception)
            {
                return;
            }

            bool errorInObject = stackTrace.Contains("Varwin.Types");
            LauncherErrorManager launcherErrorManager = LauncherErrorManager.Instance;
            VRErrorManager vrErrorManager = VRErrorManager.Instance;

            if (!errorInObject)
            {
                string userMessage = GetErrorDescByCode(ErrorCode.UnknownError);

                if (NLoggerSettings.LogDebug)
                {
                    userMessage += "\n" + condition;
                }

                if (launcherErrorManager != null)
                {
                    launcherErrorManager.ShowFatal(userMessage, stackTrace);
                }

                if (vrErrorManager != null)
                {
                    vrErrorManager.ShowFatal(userMessage, stackTrace);
                }

                LogManager.GetCurrentClassLogger().Fatal("Unknown error! " + condition + " stackTrace = " + stackTrace);
            }

            else
            {
                string userMessage = LanguageManager.Instance.GetTextValue("EXCEPTION_IN_OBJECT");
                userMessage += " " + stackTrace.Split('\n')[0];

                if (vrErrorManager != null)
                {
                    vrErrorManager.Show(userMessage);
                }

                LogManager.GetCurrentClassLogger().Fatal("Object error! " + condition + " stackTrace = " + stackTrace);
            }
        }
    }
}