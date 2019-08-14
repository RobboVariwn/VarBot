using System;
using System.Globalization;
using System.IO;
using Varwin.Errors;
using Newtonsoft.Json;
using NLog;
using SmartLocalization;
using UnityEngine;
using Varwin.Data;
using Varwin.Data.ServerData;
using Varwin.UI;

namespace Varwin
{
    public class Settings
    {
        public string RabbitMqHost;
        public string RabbitMqUserName;
        public string RabbitMqPass;
        public string RabbitMqPort;
        public string ApiHost { get; set; }
        public string Language { get; set; }
        public string StoragePath { get; set; }
        //public string TempFolder {get; set; }
        public string DebugFolder { get; set; }
        public string WebHost;
        public bool Multiplayer;
        public string PhotonHost;
        public string PhotonPort;
        public bool HighlightEnabled;
        
        //haptics settings
        public bool TouchHapticsEnabled;
        public bool GrabHapticsEnabled;
        public bool UseHapticsEnabled;

        public bool OnboardingMode;

        private static Settings _instance;

        public static void ReadTestSettings()
        {
            string json = File.ReadAllText(Application.dataPath + "/StreamingAssets/settings.txt");
            _instance = JsonConvert.DeserializeObject<Settings>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None });
            LoaderAdapter.Init(new ApiLoader());
        }

        public static Settings Instance()
        {
            return _instance;
        }

        public static void CreateStorageSettings(string folder)
        { 
            //string outFolder = Application.dataPath + "/tempUnpacked";
            LogManager.GetCurrentClassLogger().Info($"Create storage settings. Path = {folder}");
            _instance = new Settings
            {
                StoragePath = folder,
                HighlightEnabled = true,
                Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName
            };

            LoaderAdapter.Init(new StorageLoader());
             
        }

        public static void CreateDebugSettings(string path)
        {
            _instance = new Settings
            {
                DebugFolder = path,
                HighlightEnabled = true,
                TouchHapticsEnabled = true,
                GrabHapticsEnabled = true,
                UseHapticsEnabled = true,
                Language = LanguageManager.DefaultLanguage
            };

            LoaderAdapter.Init(new ApiLoader());
        }
        
        public static void ReadLauncherSettings(LaunchArguments launchArguments)
        {
            try
            {
                _instance = new Settings
                {
                    ApiHost = launchArguments.api != null ? launchArguments.api.baseUrl : _instance.ApiHost,
                    WebHost = launchArguments.web != null ? launchArguments.web.baseUrl : _instance.WebHost,
                    RabbitMqHost = launchArguments.rabbitmq != null ? launchArguments.rabbitmq.host.Split(':')[0] : _instance.RabbitMqHost,
                    RabbitMqPort = launchArguments.rabbitmq != null ? launchArguments.rabbitmq.host.Split(':')[1] : _instance.RabbitMqPort,
                    RabbitMqUserName = launchArguments.rabbitmq != null ? launchArguments.rabbitmq.login : _instance.RabbitMqUserName,
                    RabbitMqPass = launchArguments.rabbitmq != null ? launchArguments.rabbitmq.password : _instance.RabbitMqPass,
                    Language = launchArguments.lang ?? _instance.Language ?? LanguageManager.DefaultLanguage,
                    Multiplayer = false,
                    HighlightEnabled = true,
                    TouchHapticsEnabled = false,
                    GrabHapticsEnabled = false,
                    UseHapticsEnabled = false,
                    OnboardingMode = launchArguments.onboarding
                };
                
                LanguageManager.Instance.ChangeLanguage(launchArguments.lang);
            }
            catch (Exception e)
            {
               LauncherErrorManager.Instance.ShowFatal(
                   ErrorHelper.GetErrorDescByCode(Varwin.Errors.ErrorCode.ReadStartupArgsError), e.StackTrace);
            }   
            
        }

        public static void UpdateSettings(string newSettingsString)
        {
            Settings newSettings = JsonConvert.DeserializeObject<Settings>(newSettingsString, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None });
            
            _instance = new Settings
            {
                ApiHost = newSettings.ApiHost ?? _instance.ApiHost,
                WebHost = newSettings.WebHost ?? _instance.WebHost,
                RabbitMqHost = newSettings.RabbitMqHost ?? _instance.RabbitMqHost,
                RabbitMqPort = newSettings.RabbitMqPort ?? _instance.RabbitMqPort,
                RabbitMqUserName = newSettings.RabbitMqUserName ?? _instance.RabbitMqUserName,
                RabbitMqPass = newSettings.RabbitMqPass ?? _instance.RabbitMqPass,
                Language =  (newSettings.Language ?? _instance.Language) ?? LanguageManager.DefaultLanguage,
                Multiplayer = false,
                HighlightEnabled = true,
                TouchHapticsEnabled = false,
                GrabHapticsEnabled = false,
                UseHapticsEnabled = false,
                OnboardingMode = false
            };
        }

        public static void SetApiUrl(string url)
        {
            _instance.ApiHost = "http://" + url;
            _instance.RabbitMqHost = url.Split(':')[0];
        }
    }
}
