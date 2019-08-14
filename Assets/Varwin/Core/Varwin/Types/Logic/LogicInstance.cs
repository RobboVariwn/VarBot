using System;
using System.Diagnostics;
using System.Reflection;
using NLog;
using SmartLocalization;
using Varwin.Errors;
using Varwin.UI.VRErrorManager;
using Varwin.WWW;

namespace Varwin.Types
{
    public class LogicInstance
    {
        public ILogic Logic { get; private set; }

        private readonly int _worldLocationId;
        private WrappersCollection _myItems;
        private bool _initialized;

        public int WorldLocationId => _worldLocationId;

        public LogicInstance(int worldLocationId)
        {
            _worldLocationId = worldLocationId;
            GameStateData.SetLogic(this);
        }
        
        public void UpdateGroupLogic(Type newLogic)
        {
            if (newLogic == null)
            {
                LogManager.GetCurrentClassLogger().Info($"Scene template {_worldLocationId} logic is empty!");
                Clear();
                return;
            }

            _myItems = GameStateData.GetWrapperCollection();
            
            var logic = Activator.CreateInstance(newLogic) as ILogic;
            if (logic == null)
            {
                LogManager.GetCurrentClassLogger().Error($"Initialize location logic error! SceneId = {_worldLocationId}. Message: Logic is null!");
                VRErrorManager.Instance.Show(ErrorHelper.GetErrorDescByCode(Errors.ErrorCode.LogicInitError));

                return;
            }

            Logic = logic;

            try
            {
                LogManager.GetCurrentClassLogger().Info($"Scene template {_worldLocationId} logic initialize started...");
                InitializeLogic();
                LogManager.GetCurrentClassLogger().Info($"Scene template {_worldLocationId} logic initialize successful");
            }
            catch (Exception e)
            {
                ShowLogicExceptionError(Errors.ErrorCode.LogicInitError, "Initialize scene template logic error!", e);
                Logic = null;
            }
        }

        public void InitializeLogic()
        {
            _initialized = false;
            Logic.SetCollection(_myItems);
            Logic.Initialize();
            Logic.Events();
            _initialized = true;
        }

        public void ExecuteLogic()
        {
            if (Logic == null || !_initialized)
            {
                return;
            }

            try
            {
                Logic.Update();
            }
            catch (Exception e)
            {
                ShowLogicExceptionError(Errors.ErrorCode.LogicExecuteError, "Execute scene template logic error!", e);
                Logic = null;
            }
        }

        private void ShowLogicExceptionError(int errorCode, string errorMessage, Exception exception)
        {
            var logicException = new LogicException(this, exception);

            LogManager.GetCurrentClassLogger().Error($"{errorMessage} {logicException.GetStackFrameString()}");
            VRErrorManager.Instance.Show(ErrorHelper.GetErrorDescByCode(errorCode));
            AMQPClient.SendRuntimeErrorMessage(logicException);
        }

        public void Clear()
        {
            Logic = null;
        }
    }
}