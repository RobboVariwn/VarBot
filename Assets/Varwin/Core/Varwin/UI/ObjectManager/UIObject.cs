using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Varwin.UI.ObjectManager
{
    public class UIObject: MonoBehaviour
    {
        public Text ItemNameText;
        public Text ServerIdText;
        public Text GroupIdText;
        public Text ComlexIdText;
        public Text DeleteText;
        public UIDeleteButton UiDeleteButton;
        public TextMeshProUGUI TypeText;
        private GameEntity _gameEntity;
        private Transform _parenTransform;
        private ObjectController _objectController; 
        private float _height;

        public void Init(Transform parent, ObjectController objectController)
        {
            _objectController = objectController;
            _gameEntity = objectController.Entity;
            _parenTransform = parent;
            objectController.UiObject = this;
            MoveUi();
            UiDeleteButton.Init(objectController);
            _height = CalculateHeight();
            gameObject.SetActive(false);

            if (Settings.Instance().OnboardingMode)
            {
                UiDeleteButton.gameObject.SetActive(false);
                DeleteText.gameObject.SetActive(false);
            }
        }

        private float CalculateHeight()
        {
            List<float> heights = new List<float>();
            Transform[] childrens = _parenTransform.gameObject.GetComponentsInChildren<Transform>();

            foreach (Transform children in childrens)
            {
                Renderer component = children.gameObject.GetComponent<Renderer>();
                if (component == null)
                {
                    continue;
                }

                heights.Add(component.bounds.size.y);
            }

            if (heights.Count == 0)
            {
                Debug.Log($"GameObject {_gameEntity.name.Value} have no any Renderer!");
            }

            float maxHeight = heights.Max();
            //maxHeight /= _parenTransform.transform.localScale.y;

            return maxHeight;
        }

        public ObjectController GetBaseType() => _objectController;

        public void MoveUi()
        {
            if (_gameEntity.gameObject == null)
            {
                Debug.Log("!_object.gameObject == null");
                return;
            }

            try
            {
                Vector3 position = _objectController.gameObject.transform.position;

                gameObject.transform.position =
                    new Vector3(position.x, position.y + _height,
                        position.z);
            }
            catch (Exception e)
            {
                Debug.Log("!" + e.Message);
            }

            

        }

        public void Update()
        {
            if (_gameEntity == null)
            {
                return;
            }

            if (_gameEntity.id.Value == 0)
            {
                gameObject.SetActive(false);
            }

            ItemNameText.text = $"{_gameEntity.name.Value}";
            ServerIdText.text = $"server_id = {_gameEntity.idServer.Value}";
            TypeText.text = _objectController.GetLocalizedName();
            //GroupIdText.text = $"G:{_objectController.IdGroup}";
            ComlexIdText.text = $"Free";
            MoveUi();
            gameObject.transform.LookAt(GameObjects.Instance.Head);
        }
    }

    
}
