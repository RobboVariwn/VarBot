using System.Collections.Generic;
using UnityEngine;
using Varwin.WWW;

namespace Varwin
{

    public static class LogicBlockLogger
    {
        private static BlockLoggerInstance _instance;

        public static BlockLoggerInstance Instance
        {
            get
            {
                if ((Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor) && _instance == null)
                {
                    _instance = Object.FindObjectOfType<BlockLoggerInstance>();
                }

                if (_instance == null)
                {
                    GameObject go = new GameObject();
                    _instance = go.AddComponent<BlockLoggerInstance>();
                    go.name = "BlocklyLogger";
                    Object.DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }

        public static void LogBlock(string block)
        {
            Instance.LogBlockString(block);
        }

        public static void ForceLogSend()
        {
            Instance.SendBlocksForced();
        }
    }

    public class BlockLoggerInstance : MonoBehaviour
    {
        
        private const float SendBlocksTimerPeriod = 2.0f;
 
        private float _timer;
        private List<string> _blocks;
        private bool _forcedSend;
        private List<string> _forcedBlocks;

        void Awake()
        {
            _blocks = new List<string>();
            _forcedBlocks = new List<string>();
        }

        private void LateUpdate()
        {
            if (ProjectData.GameMode != GameMode.Preview)
            {
                return;
            }
            
            _timer += Time.deltaTime;

            if (_forcedSend)
            {
                _forcedBlocks.AddRange(_blocks);
                _forcedSend = false;
            }

            if (_timer >= SendBlocksTimerPeriod)
            {
                SendBlocks();
                _timer = 0.0f;
            }

            _blocks.Clear();
        }

        private void SendBlocks()
        {
            _blocks.AddRange(_forcedBlocks);
            AMQPClient.SendRuntimeBlocks(ProjectData.SceneId, _blocks.ToArray());
            _forcedBlocks.Clear();
        }

        public void LogBlockString(string block)
        {
            _blocks.Add(block);
        }

        public void SendBlocksForced()
        {
            _forcedSend = true;
        }

    }
}