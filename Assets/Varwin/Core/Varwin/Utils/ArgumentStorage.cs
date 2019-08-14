using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog;
using UnityEngine;
using Logger = NLog.Logger;

namespace Varwin
{
    public static class ArgumentStorage
    {
        public static readonly Dictionary<string, string> Args = new Dictionary<string, string>();

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static string GetValue(string key)
        {
            _logger.Info("##" + key);
            _logger.Info("$$" + Args[key]);

            return Args[key];
        }

        public static void SetValue(string key, string value)
        {
            _logger.Info("!!" + key);
            _logger.Info("@@" + value);
            Args[key] = value;
        }

        public static void ClearStorage()
        {
            Args.Clear();
        }

        public static void AddJsonArgsArray(string args)
        {
            var jObj = JObject.Parse(args);

            foreach (JProperty property in jObj.Properties())
            {
                SetValue(property.Name, property.Value.ToString());
            }
        }
    }
}