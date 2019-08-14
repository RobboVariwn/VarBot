using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Varwin.Errors;
using NLog;
using Varwin.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UnityEngine;
using Varwin.Data.ServerData;
using Varwin.ECS.Systems.Group;
using Varwin.Models.Data;
using Varwin.Types;
using Varwin.UI;
using Varwin.UI.VRErrorManager;

namespace Varwin.WWW
{
    // ReSharper disable once InconsistentNaming
    public static class AMQPClient
    {
        private static IConnection _connection;
        private static IModel _chanel;
        private static readonly Dictionary<string, string> Exchanges = new Dictionary<string, string>();
        private static Rabbitmq _settings;
        private static string _cachePath;
        private static readonly Dictionary<int, string> SubscribeCompilationTags = new Dictionary<int, string>();
        private static readonly Dictionary<int, string> SubscribeLogicTags = new Dictionary<int, string>();
        private static readonly Dictionary<int, string> SubscribeObjectsChangesTags = new Dictionary<int, string>();

        private static readonly Dictionary<int, List<string>> SubscribeSceneChangesTags =
            new Dictionary<int, List<string>>();

        private static readonly Dictionary<int, List<string>> SubscribeProjectConfigurationChangesTags =
            new Dictionary<int, List<string>>();

        private static string ExchangeDeclareIfNotExist(
            string exchange,
            string type,
            bool durable = false,
            bool autoDelete = false,
            IDictionary<string, object> arguments = null)
        {
            if (Exchanges.ContainsKey(exchange))
            {
                return Exchanges[exchange];
            }


            GetChanel()
                .ExchangeDeclare(exchange,
                    type,
                    durable,
                    autoDelete,
                    arguments);
            Exchanges.Add(exchange, GetChanel().QueueDeclare().QueueName);

            return Exchanges[exchange];
        }

        private static IConnection GetConnectionChanel()
        {
            if (_connection != null)
            {
                return _connection;
            }

            if (_settings == null)
            {
                const string message = "Settings for RabbitMQ is not initialized!";
                LogManager.GetCurrentClassLogger().Fatal(message);
                VRErrorManager.Instance.ShowFatal(message);

                return null;
            }

            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = _settings.host.Split(':')[0],
                UserName = _settings.login,
                Password = _settings.password,
                Port = Convert.ToInt32(_settings.host.Split(':')[1])
            };

            try
            {
                _connection = factory.CreateConnection();
            }
            catch (Exception e)
            {
                string message = "Can't connect to RabbitMQ! " + e.Message;
                LogManager.GetCurrentClassLogger().Fatal(message);
                VRErrorManager.Instance.ShowFatal(message);
            }

            return _connection;
        }

        private static IModel GetChanel() => _chanel ?? (_chanel = GetConnectionChanel().CreateModel());

        public static void CloseConnection()
        {
            _connection?.Close();
        }

        public static void SubscribeSceneChange(int projectId)
        {
            LogManager.GetCurrentClassLogger().Info($"subscribe scene change to project {projectId}");
            IModel channel = GetChanel();
            string queueNameAdd = ExchangeDeclareIfNotExist(ExchangeNames.SceneAdded, "topic", true);
            string key = $"project.{projectId}";

            channel.QueueBind(queueNameAdd,
                ExchangeNames.SceneAdded,
                key);
            EventingBasicConsumer consumerForAdd = new EventingBasicConsumer(channel);

            consumerForAdd.Received += (model, ea) =>
            {
                LogManager.GetCurrentClassLogger().Info("Scene was added");
                var body = ea.Body;
                string jsonData = Encoding.UTF8.GetString(body);
                ProjectSceneWithPrefabObjects location = jsonData.JsonDeserialize<ProjectSceneWithPrefabObjects>();

                ProjectDataListener.Instance.ProjectSceneArguments = new ProjectSceneArguments()
                {
                    Scene = location.Scene,
                    Location = location.Location,
                    State = ProjectSceneArguments.StateProjectScene.Added
                };
            };


            string queueNameDelete = ExchangeDeclareIfNotExist(ExchangeNames.SceneDeleted, "topic", true);
            key = $"project.{projectId}";

            channel.QueueBind(queueNameDelete,
                ExchangeNames.SceneDeleted,
                key);
            EventingBasicConsumer consumerForDelete = new EventingBasicConsumer(channel);

            consumerForDelete.Received += (model, ea) =>
            {
                LogManager.GetCurrentClassLogger().Info("Location was deleted");
                var body = ea.Body;
                string jsonData = Encoding.UTF8.GetString(body);
                ProjectSceneWithPrefabObjects location = jsonData.JsonDeserialize<ProjectSceneWithPrefabObjects>();

                ProjectDataListener.Instance.ProjectSceneArguments = new ProjectSceneArguments()
                {
                    Scene = location.Scene,
                    Location = location.Location,
                    State = ProjectSceneArguments.StateProjectScene.Deleted
                };
            };


            string queueNameChange = ExchangeDeclareIfNotExist(ExchangeNames.SceneChanged, "topic", true);
            key = $"project.{projectId}";

            channel.QueueBind(queueNameChange,
                ExchangeNames.SceneChanged,
                key);
            EventingBasicConsumer consumerForChange = new EventingBasicConsumer(channel);

            consumerForChange.Received += (model, ea) =>
            {
                LogManager.GetCurrentClassLogger().Info("Location was changed");
                var body = ea.Body;
                string jsonData = Encoding.UTF8.GetString(body);
                ProjectSceneWithPrefabObjects location = jsonData.JsonDeserialize<ProjectSceneWithPrefabObjects>();

                ProjectDataListener.Instance.ProjectSceneArguments = new ProjectSceneArguments()
                {
                    Scene = location.Scene,
                    Location = location.Location,
                    State = ProjectSceneArguments.StateProjectScene.Changed
                };
            };

            List<string> tags = new List<string>
            {
                channel.BasicConsume(queueNameDelete, true, consumerForDelete),
                channel.BasicConsume(queueNameAdd, true, consumerForAdd),
                channel.BasicConsume(queueNameChange, true, consumerForChange)
            };

            if (!SubscribeSceneChangesTags.ContainsKey(projectId))
            {
                SubscribeSceneChangesTags.Add(projectId, null);
            }

            SubscribeSceneChangesTags[projectId] = tags;
        }

        public static void UnSubscribeProjectSceneChange(int projectId)
        {
            if (!SubscribeSceneChangesTags.ContainsKey(projectId))
            {
                return;
            }

            LogManager.GetCurrentClassLogger()
                .Info($"Unsubscribe locations change to projectId {projectId} started...");

            Exchanges.Remove(ExchangeNames.SceneAdded);
            Exchanges.Remove(ExchangeNames.SceneDeleted);
            Exchanges.Remove(ExchangeNames.SceneChanged);

            IModel channel = GetChanel();
            List<string> tags = SubscribeSceneChangesTags[projectId];

            foreach (var tag in tags)
            {
                channel.BasicCancel(tag);
            }

            SubscribeSceneChangesTags.Remove(projectId);
            LogManager.GetCurrentClassLogger().Info("Unsubscribe scene successful");
        }

        public static void SubscribeProjectConfigurationChange(int projectId)
        {
            LogManager.GetCurrentClassLogger().Info($"subscribe configuration change to project {projectId}");
            IModel channel = GetChanel();
            string queueNameAdd = ExchangeDeclareIfNotExist(ExchangeNames.ProjectConfigurationAdded, "topic", true);
            string key = $"project.{projectId}";

            channel.QueueBind(queueNameAdd,
                ExchangeNames.ProjectConfigurationAdded,
                key);
            EventingBasicConsumer consumerForAdd = new EventingBasicConsumer(channel);

            consumerForAdd.Received += (model, ea) =>
            {
                LogManager.GetCurrentClassLogger().Info("Configuration was added");
                var body = ea.Body;
                string jsonData = Encoding.UTF8.GetString(body);
                ProjectConfiguration configuration = jsonData.JsonDeserialize<ProjectConfiguration>();

                ProjectDataListener.Instance.ProjectConfigurationArguments = new ProjectConfigurationArguments()
                {
                    ProjectConfiguration = configuration,
                    State = ProjectConfigurationArguments.StateConfiguration.Added
                };
            };


            string queueNameDelete =
                ExchangeDeclareIfNotExist(ExchangeNames.ProjectConfigurationDeleted, "topic", true);
            key = $"project.{projectId}";

            channel.QueueBind(queueNameDelete,
                ExchangeNames.ProjectConfigurationDeleted,
                key);
            EventingBasicConsumer consumerForDelete = new EventingBasicConsumer(channel);

            consumerForDelete.Received += (model, ea) =>
            {
                LogManager.GetCurrentClassLogger().Info("Configuration was deleted");
                var body = ea.Body;
                string jsonData = Encoding.UTF8.GetString(body);
                ProjectConfiguration configuration = jsonData.JsonDeserialize<ProjectConfiguration>();

                ProjectDataListener.Instance.ProjectConfigurationArguments = new ProjectConfigurationArguments()
                {
                    ProjectConfiguration = configuration,
                    State = ProjectConfigurationArguments.StateConfiguration.Deleted
                };
            };


            string queueNameChange =
                ExchangeDeclareIfNotExist(ExchangeNames.ProjectConfigurationChanged, "topic", true);
            key = $"project.{projectId}";

            channel.QueueBind(queueNameChange,
                ExchangeNames.ProjectConfigurationChanged,
                key);
            EventingBasicConsumer consumerForChange = new EventingBasicConsumer(channel);

            consumerForChange.Received += (model, ea) =>
            {
                LogManager.GetCurrentClassLogger().Info("Configuration was changed");
                var body = ea.Body;
                string jsonData = Encoding.UTF8.GetString(body);
                ProjectConfiguration configuration = jsonData.JsonDeserialize<ProjectConfiguration>();

                ProjectDataListener.Instance.ProjectConfigurationArguments = new ProjectConfigurationArguments()
                {
                    ProjectConfiguration = configuration,
                    State = ProjectConfigurationArguments.StateConfiguration.Changed
                };
            };

            List<string> tags = new List<string>
            {
                channel.BasicConsume(queueNameDelete, true, consumerForDelete),
                channel.BasicConsume(queueNameAdd, true, consumerForAdd),
                channel.BasicConsume(queueNameChange, true, consumerForChange)
            };

            if (!SubscribeProjectConfigurationChangesTags.ContainsKey(projectId))
            {
                SubscribeProjectConfigurationChangesTags.Add(projectId, null);
            }

            SubscribeProjectConfigurationChangesTags[projectId] = tags;
        }

        public static void UnSubscribeProjectConfigurationsChange(int projectId)
        {
            if (!SubscribeProjectConfigurationChangesTags.ContainsKey(projectId))
            {
                return;
            }

            LogManager.GetCurrentClassLogger()
                .Info($"Unsubscribe configuration change to projectId {projectId} started...");

            Exchanges.Remove(ExchangeNames.ProjectConfigurationAdded);
            Exchanges.Remove(ExchangeNames.ProjectConfigurationDeleted);
            Exchanges.Remove(ExchangeNames.ProjectConfigurationChanged);

            IModel channel = GetChanel();
            List<string> tags = SubscribeProjectConfigurationChangesTags[projectId];

            foreach (var tag in tags)
            {
                channel.BasicCancel(tag);
            }

            SubscribeProjectConfigurationChangesTags.Remove(projectId);
            LogManager.GetCurrentClassLogger().Info("Unsubscribe project configuration successful");
        }

        public static void SubscribeObjectChange(int projectId, int sceneId)
        {
            LogManager.GetCurrentClassLogger()
                .Info($"subscribe object change to location {sceneId}, project {projectId}");
            IModel channel = GetChanel();
            string queueName = ExchangeDeclareIfNotExist(ExchangeNames.SceneObjectChanged, "topic", true);
            string key = $"project.{projectId}.scene.{sceneId}";

            channel.QueueBind(queueName,
                ExchangeNames.SceneObjectChanged,
                key);
            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                LogManager.GetCurrentClassLogger().Info("Object was changed");
                var body = ea.Body;
                string jsonData = Encoding.UTF8.GetString(body);
                ObjectDto objectDto = jsonData.JsonDeserialize<ObjectDto>();
                ProjectDataListener.Instance.UpdateObject(objectDto);
            };

            string tag = channel.BasicConsume(queueName, true, consumer);

            if (!SubscribeObjectsChangesTags.ContainsKey(sceneId))
            {
                SubscribeObjectsChangesTags.Add(sceneId, string.Empty);
            }

            SubscribeObjectsChangesTags[sceneId] = tag;
        }

        public static void UnSubscribeObjectChange(int sceneId)
        {
            if (!SubscribeObjectsChangesTags.ContainsKey(sceneId))
            {
                return;
            }

            LogManager.GetCurrentClassLogger().Info($"Unsubscribe object change to location {sceneId} started...");
            Exchanges.Remove(ExchangeNames.SceneObjectChanged);
            IModel channel = GetChanel();
            channel.BasicCancel(SubscribeObjectsChangesTags[sceneId]);
            SubscribeObjectsChangesTags.Remove(sceneId);
            LogManager.GetCurrentClassLogger().Info("Unsubscribe logic successful");
        }

        public static void SubscribeLogicChange(int sceneId)
        {
            try
            {
                LogManager.GetCurrentClassLogger().Info($"subscribe logic change to location {sceneId}");

                IModel channel = GetChanel();
                string queueName = ExchangeDeclareIfNotExist(ExchangeNames.LogicChanged, "topic", true);

                string key = $"scene.{sceneId}";

                channel.QueueBind(queueName,
                    ExchangeNames.LogicChanged,
                    key);

                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    LogManager.GetCurrentClassLogger().Info("New logic code was received");
                    var body = ea.Body;
                    LogicUpdate groupLogicUpdate = new LogicUpdate(Contexts.sharedInstance, body, sceneId);
                    groupLogicUpdate.Execute();
                };

                string tag = channel.BasicConsume(queueName,
                    true,
                    consumer);

                if (!SubscribeLogicTags.ContainsKey(sceneId))
                {
                    SubscribeLogicTags.Add(sceneId, string.Empty);
                }

                SubscribeLogicTags[sceneId] = tag;
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Fatal($"RabbitMQ error! subscribe to logic error! {ex.Message}");
                VRErrorManager.Instance.ShowFatal("RabbitMQ error! Subscribe to logic error!");
            }
        }

        public static void UnSubscribeLogicChange(int sceneId)
        {
            LogManager.GetCurrentClassLogger().Info($"Unsubscribe logic change to location {sceneId} started...");

            if (!SubscribeLogicTags.ContainsKey(sceneId))
            {
                return;
            }

            Exchanges.Remove(ExchangeNames.LogicChanged);

            IModel channel = GetChanel();
            channel.BasicCancel(SubscribeLogicTags[sceneId]);

            SubscribeLogicTags.Remove(sceneId);
            LogManager.GetCurrentClassLogger().Info("Unsubscribe logic successful");
        }

        public static void SubscribeCompilationError(int sceneId)
        {
            try
            {
                LogManager.GetCurrentClassLogger().Info($"subscribe compilation error to scene {sceneId}");

                IModel channel = GetChanel();
                string queueName = ExchangeDeclareIfNotExist(ExchangeNames.CompilationError, "topic", true);

                string key = $"scene.{sceneId}";

                channel.QueueBind(queueName,
                    ExchangeNames.CompilationError,
                    key);

                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    LogManager.GetCurrentClassLogger().Info("Compilation error was received");

                    LogicUpdate groupLogicUpdate = new LogicUpdate(Contexts.sharedInstance, null, sceneId);
                    groupLogicUpdate.Execute();
                    GameStateData.ClearLogic();
                };

                string tag = channel.BasicConsume(queueName,
                    true,
                    consumer);

                if (!SubscribeCompilationTags.ContainsKey(sceneId))
                {
                    SubscribeCompilationTags.Add(sceneId, string.Empty);
                }

                SubscribeCompilationTags[sceneId] = tag;
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger()
                    .Fatal($"RabbitMQ error! subscribe to compilation error! {ex.Message}");
                VRErrorManager.Instance.ShowFatal("RabbitMQ error! Subscribe to compilation error!");
            }
        }

        public static void UnSubscribeCompilationError(int sceneId)
        {
            LogManager.GetCurrentClassLogger().Info($"Unsubscribe compilation error to scene {sceneId} started...");

            if (!SubscribeCompilationTags.ContainsKey(sceneId))
            {
                return;
            }

            Exchanges.Remove(ExchangeNames.CompilationError);

            IModel channel = GetChanel();
            channel.BasicCancel(SubscribeCompilationTags[sceneId]);

            SubscribeCompilationTags.Remove(sceneId);
            LogManager.GetCurrentClassLogger().Info("Unsubscribe compilation error successful");
        }

        public static void SendRuntimeErrorMessage(LogicException logicException)
        {
            SendRuntimeErrorMessage(logicException.Logic.WorldLocationId,
                logicException.Line,
                logicException.Column,
                logicException.Message);
        }

        public static void SendRuntimeErrorMessage(
            int sceneId,
            int line,
            int column,
            string errorMessage)
        {
            IModel channel = GetChanel();
            ExchangeDeclareIfNotExist(ExchangeNames.RuntimeError, "topic", true);
            string key = $"project.{ProjectData.ProjectId}.{sceneId}";

            LogicBlockError error = new LogicBlockError
            {
                SceneId = sceneId, Line = line, Column = column, ErrorMessage = errorMessage
            };

            var message = Encoding.ASCII.GetBytes(error.ToJson());

            channel.BasicPublish(ExchangeNames.RuntimeError,
                key,
                false,
                null,
                message);
        }

        public static void SendRuntimeBlocks(
            int sceneId,
            string[] blocks)
        {
            IModel channel = GetChanel();
            ExchangeDeclareIfNotExist(ExchangeNames.RuntimeBlocks, "topic", true);
            string key = $"project.{ProjectData.ProjectId}.{sceneId}";

            LogicBlockRuntimeList list = new LogicBlockRuntimeList {SceneId = sceneId, Blocks = blocks};

            var message = Encoding.ASCII.GetBytes(list.ToJson());

            channel.BasicPublish(ExchangeNames.RuntimeBlocks,
                key,
                false,
                null,
                message);
        }

        public static void ReadLaunchArgs()
        {
            try
            {
                string key = _settings.key;
                string queueName = ExchangeNames.Launch + "_" + key;
                string exchangeName = ExchangeNames.Launch;

                IModel channel = GetChanel();
                ExchangeDeclareIfNotExist(exchangeName, "direct", true);

                channel.QueueBind(queueName,
                    exchangeName,
                    key);

                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    LogManager.GetCurrentClassLogger().Info("vw.launch received message!");
                    var body = ea.Body;
                    string message = Encoding.UTF8.GetString(body);

#if UNITY_EDITOR
                    SaveCache(message);
#endif
                    LaunchArguments launchArguments = message.JsonDeserialize<LaunchArguments>();

                    if (launchArguments.extraArgs != null)
                    {
                        Debug.Log("!!@@##"
                                  + launchArguments
                                      .extraArgs); //not sure where to instantiate local logger with LogManager.GetCurrentClassLogger()
                        ArgumentStorage.ClearStorage();
                        ArgumentStorage.AddJsonArgsArray(launchArguments.extraArgs);
                    }

                    ProjectDataListener.Instance.LaunchArguments = launchArguments;
                };

                channel.BasicConsume(queueName,
                    true,
                    consumer);
            }
            catch (Exception e)
            {
                CloseConnectionAndChanel();

#if UNITY_EDITOR
                LaunchArguments launchArguments = LoadFromCache();

                if (launchArguments != null)
                {
                    LogManager.GetCurrentClassLogger().Info("Loading previous vw.launch...");
                    ProjectDataListener.Instance.LaunchArguments = launchArguments;
                }
                else
                {
                    LauncherErrorManager.Instance.ShowFatalErrorKey(
                        ErrorHelper.GetErrorKeyByCode(Varwin.Errors.ErrorCode.RabbitNoArgsError),
                        e.StackTrace);
                }

#else
                LogManager.GetCurrentClassLogger().Fatal("vw.launch not found! StackTrace = " + e.StackTrace);
                LauncherErrorManager.Instance.ShowFatalErrorKey(ErrorHelper.GetErrorKeyByCode(Varwin.Errors.ErrorCode.RabbitNoArgsError), e.StackTrace);
#endif
            }
        }

        private static void CloseConnectionAndChanel()
        {
            _connection.Close();
            _connection = null;

            _chanel.Close();
            _chanel = null;
        }

        private static void SaveCache(string message)
        {
            if (!Directory.Exists(_cachePath))
            {
                Directory.CreateDirectory(_cachePath);
            }

            File.WriteAllText(_cachePath + "/launch.args", message);
        }

        private static LaunchArguments LoadFromCache()
        {
            if (!File.Exists(_cachePath + "/launch.args"))
            {
                return null;
            }

            string message = File.ReadAllText(_cachePath + "/launch.args");

            try
            {
                LaunchArguments launchArguments = message.JsonDeserialize<LaunchArguments>();

                return launchArguments;
            }
            catch
            {
                return null;
            }
        }

        public static void Init(Rabbitmq rabbitmq)
        {
            _settings = rabbitmq;
            _cachePath = Application.persistentDataPath + "/cache";
        }
    }
}
