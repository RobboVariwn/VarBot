using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NLog;
using Varwin.WWW;
using UnityEngine;
using Varwin.Data;
using Varwin.Data.ServerData;
using Varwin.WWW.Models;
using Logger = NLog.Logger;

namespace Varwin.WWW
{
    public static class API
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        public static void GetAndUpdateServerSettings(Action<bool> onFinish = null)
        {
            RequestApi requestServerSettings = new RequestApi(ApiRoutes.ServerConfig);
            requestServerSettings.TimeOut = 10f;

            requestServerSettings.OnFinish = response =>
            {
                ResponseApi settingsResponse = (ResponseApi) response;
                if (!Helper.IsResponseGood(settingsResponse))
                {
                    Logger.Fatal($"Can not get {ApiRoutes.ServerConfig}");
                    onFinish?.Invoke(true);
                    return;
                }

                Settings.UpdateSettings(settingsResponse.Data.ToString());

                onFinish?.Invoke(true);
            };

            requestServerSettings.OnError = s =>
            {
                Debug.LogError($"Can't get server settings: {s}");

                onFinish?.Invoke(false);
            };
        }
        
        public static void GetProjects(Action<List<ProjectItem>> onFinish = null, bool onlyMobile = false)
        {
            string url = onlyMobile ? ApiRoutes.MobileProjects : ApiRoutes.Projects; 
            RequestApi requestProjects = new RequestApi(url);
            requestProjects.TimeOut = 10f;

            requestProjects.OnFinish = response =>
            {
                ResponseApi projectsResponse = (ResponseApi) response;
                if (!Helper.IsResponseGood(projectsResponse))
                {
                    Logger.Fatal($"Can not get {url}");
                    onFinish?.Invoke(null);
                    return;
                }

                string projectsJson = projectsResponse.Data.ToString();

                var projects = projectsJson.JsonDeserializeList<ProjectItem>();
                onFinish?.Invoke(projects);
            };

            requestProjects.OnError = s =>
            {
                Debug.LogError($"Can't get projects list: {s}");

                onFinish?.Invoke(null);
            };
        }
        
        public static void GetObjects(int offset, int limit, string search, bool onlyMobile = false, Action<List<PrefabObject>> callback = null)
        {
            var url = onlyMobile ? ApiRoutes.MobileObjects : ApiRoutes.Objects;
            url = string.Format(url, offset, limit, search);
            RequestApi requestApi = new RequestApi(url);

            requestApi.OnFinish = response =>
            {
                ResponseApi responseApi = (ResponseApi) response;
                string json = responseApi.Data.ToString();
                var result = json.JsonDeserializeList<PrefabObject>();
                callback?.Invoke(result);
            };

            requestApi.OnError = message =>
            {
                Debug.LogError($"Can't get objects list: {message}");
                callback?.Invoke(new List<PrefabObject>());
            };
        }
    }
}