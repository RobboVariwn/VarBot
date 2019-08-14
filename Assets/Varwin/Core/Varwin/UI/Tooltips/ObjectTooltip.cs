using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Varwin.UI
{
    public class ObjectTooltip : MonoBehaviour
    {       
        public enum ObjectTooltipSize
        {
            Small,
            Large
        }
        
        private GameObject _tooltippedObject;
        private string _tooltipText;

        private LineRenderer _line;
        private Text _text;
        private Transform _canvasTransform;

        private int resizeTextMinSizeDefault;
        private int resizeTextMaxSizeDefault;
       
        private float _verticalOffset;

        private MeshRenderer[] _meshRenderers;
        private SkinnedMeshRenderer[] _skinnedMeshRenderers;

        private bool _initialized;
      
        [SerializeField]
        private float _maxTextHeight = 125.0f;

        [HideInInspector]
        public ObjectTooltipSize TooltipSize = ObjectTooltipSize.Large;
        
        public GameObject TooltippedObject => _tooltippedObject;

        private void Start()
        {
            _line = GetComponentInChildren<LineRenderer>();
            _text = GetComponentInChildren<Text>();

            resizeTextMinSizeDefault = _text.resizeTextMinSize;
            resizeTextMaxSizeDefault = _text.resizeTextMaxSize;
            
            _canvasTransform = GetComponentInChildren<Canvas>().transform;

            _line.material.color = Color.black;

            _line.enabled = false;
        }

        public void SetTooltip(GameObject o, string text, ObjectTooltipSize tooltipSize, float verticalOffset)
        {
            _tooltippedObject = o;
            _tooltipText = text;
            TooltipSize = tooltipSize;
            _verticalOffset = verticalOffset;

            UpdateObjectElements();

            _initialized = (_tooltippedObject != null && text.Trim().Length != 0);
        }

        private void Update()
        {
            UpdateTooltip();
        }

        private void UpdateTooltip()
        {

            _line.enabled = _initialized;
            _canvasTransform.gameObject.SetActive(_initialized);

            if (!_initialized)
            {
                return;
            }
            else
            {
                if (_tooltippedObject == null)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            

            GetObjectLineCenterAndPivot(out Vector3 center, out Vector3 pivot);

            _canvasTransform.position = pivot + Vector3.up * _verticalOffset;

            _line.SetPosition(0, _canvasTransform.position);
            _line.SetPosition(1, center);

            _text.text = _tooltipText;

            _text.verticalOverflow = VerticalWrapMode.Overflow;

            float textHeight = _text.preferredHeight;
            
            if (textHeight > _maxTextHeight)
            {
                textHeight = _maxTextHeight;

                _text.resizeTextMinSize = resizeTextMinSizeDefault;
                _text.resizeTextMaxSize = resizeTextMaxSizeDefault;
                _text.resizeTextForBestFit = true;
            }
            else
            {
                _text.resizeTextForBestFit = false;
            }

            RectTransform textRectTransform = _text.rectTransform;

            textRectTransform.sizeDelta = new Vector2(textRectTransform.sizeDelta.x, textHeight);

            _text.verticalOverflow =
                (textHeight == _maxTextHeight) ? VerticalWrapMode.Truncate : VerticalWrapMode.Overflow;
            
            
            _canvasTransform.LookAt(GameObjects.Instance.Head);
        }

        private void GetObjectLineCenterAndPivot(out Vector3 center, out Vector3 pivot)
        {

            pivot = Vector3.zero;

            float maxY = Mathf.NegativeInfinity;

            int validRederersCounter = 0;
            
            if (_meshRenderers.Length == 0 && _skinnedMeshRenderers.Length == 0)
            {
                pivot = _tooltippedObject.transform.position;
                center = pivot;

                return;
            }

            foreach (var meshRenderer in _meshRenderers)
            {
                if (meshRenderer == null)
                {
                    continue;
                }
                
                maxY = Mathf.Max(maxY, meshRenderer.bounds.max.y);
                pivot += meshRenderer.bounds.center;

                validRederersCounter++;
            }

            foreach (var skinnedMeshRenderer in _skinnedMeshRenderers)
            {
                if (skinnedMeshRenderer == null)
                {
                    continue;
                }
                
                maxY = Mathf.Max(maxY, skinnedMeshRenderer.bounds.max.y);
                pivot += skinnedMeshRenderer.bounds.center;

                validRederersCounter++;
            }

            pivot /= validRederersCounter;

            center = pivot;

            pivot.y = maxY;

            if (validRederersCounter != _meshRenderers.Length + _skinnedMeshRenderers.Length)
            {
                UpdateObjectElements();
            }
        }

        public void UpdateObjectElements()
        {
            _meshRenderers = _tooltippedObject?.GetComponentsInChildren<MeshRenderer>();
            _skinnedMeshRenderers = _tooltippedObject?.GetComponentsInChildren<SkinnedMeshRenderer>();
        }

    }
}