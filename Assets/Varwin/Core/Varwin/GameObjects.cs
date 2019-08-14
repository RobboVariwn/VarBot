using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Varwin.UI;
using Varwin.VRInput;
#if UNITY_STANDALONE_WIN && !VRMAKER
using ZenFulcrum.EmbeddedBrowser;

#endif

namespace Varwin
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GameObjects : MonoBehaviour
    {
        public static GameObjects Instance;

        #region Player Transforms

        public Transform PlayerRig;
        public Transform EditPoint;
        public Transform SpawnPoint;
        public Transform Head;
        public Transform LeftHand;
        public Transform RightHand;
        public Transform TipAttach;

        #endregion

        #region UI prefabs

        public GameObject UIID;
        public GameObject UIObject;
        public GameObject UIToolTip;
        public UIMenu UiMenu;
#if UNITY_STANDALONE_WIN && !VRMAKER
        public VRMainControlPanel VrMainControlPanel;
#endif
        public GameObject Load;

        #endregion

        public Dictionary<string, GameObject> MagnetObjects = new Dictionary<string, GameObject>();

        private void Awake()
        {
            Instance = this;

            if (BrowserDestructor.Instance == null)
            {
                GameObject browserDesctructor = Instantiate(new GameObject("Browser destructor"));
                browserDesctructor.AddComponent<BrowserDestructor>();
                DontDestroyOnLoad(browserDesctructor);
            }

#if UNITY_STANDALONE_WIN && !VRMAKER
            var destructor = FindObjectOfType<BrowserDestructor>();
            destructor.Init(VrMainControlPanel);
            PointerUIBase.RightHand = null;
#endif
        }

        private void Start()
        {
            StartCoroutine(WaitPointer());
        }

        private IEnumerator WaitPointer()
        {
            GameObject pointer = null;

            while (pointer == null)
            {
                yield return new WaitForEndOfFrame();
                pointer = InputAdapter.Instance.PlayerController.Nodes.RightHand.GameObject;
            }

#if UNITY_STANDALONE_WIN && !VRMAKER
            while (PointerUIBase.RightHand == null)
            {
                yield return new WaitForEndOfFrame();
                PointerUIBase.RightHand = pointer.transform;
            }
#endif

            yield return true;
        }
    }
}