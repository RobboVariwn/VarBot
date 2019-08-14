using System;
using UnityEngine;
using UnityEngine.Events;

namespace Varwin.Public
{
    [RequireComponent(typeof(Rigidbody))]
    //[RequireComponent(typeof(VarwinObjectDescriptor))]
    public class InteractableObjectBehaviour : MonoBehaviour, IGrabStartAware, IGrabEndAware, IUseStartAware,
        IUseEndAware, ITouchStartAware, ITouchEndAware
    {
        [Header("Interaction settings")]
        [SerializeField]
        private bool _isGrabbable = true;

        [SerializeField]
        private bool _isUsable = true;

        [SerializeField]
        private bool _isTouchable = true;

        private IWrapperAware _wrapperAware;

        private void Create()
        {
            _wrapperAware = GetComponentInChildren<IWrapperAware>();
        }

        public bool IsGrabbable
        {
            get { return _isGrabbable; }
            set
            {
                if (_isGrabbable == value)
                {
                    return;
                }

                _isGrabbable = value;

                if (value)
                {
                    _wrapperAware.EnableInputGrab(gameObject);
                }
                else
                {
                    _wrapperAware.DisableInputGrab(gameObject);
                }
            }
        }

        public bool IsUsable
        {
            get { return _isUsable; }
            set
            {
                if (_isUsable == value)
                {
                    return;
                }

                _isUsable = value;

                if (value)
                {
                    _wrapperAware.EnableInputUsing(gameObject);
                }
                else
                {
                    _wrapperAware.DisableInputUsing(gameObject);
                }
            }
        }

        public bool IsTouchable
        {
            get { return _isTouchable; }
            set
            {
                if (_isTouchable == value)
                {
                    return;
                }

                _isTouchable = value;

                if (value)
                {
                    _wrapperAware.EnableTouch(gameObject);
                }
                else
                {
                    _wrapperAware.DisableTouch(gameObject);
                }
            }
        }

        [NonSerialized]
        public bool IsGrabbed = false;

        [NonSerialized]
        public bool IsUsed = false;

        [NonSerialized]
        public bool IsTouched = false;


        [Space(5)]
        [Header("Events")]
        public UnityEvent OnGrabStarted;

        public UnityEvent OnGrabEnded;
        public UnityEvent OnUseStarted;
        public UnityEvent OnUseEnded;
        public UnityEvent OnTouchStarted;
        public UnityEvent OnTouchEnded;

        public void OnGrabStart(GrabingContext context)
        {
            if (!_isGrabbable)
            {
                return;
            }
            
            IsGrabbed = true;
            OnGrabStarted?.Invoke();
        }

        public void OnGrabEnd()
        {
            if (!_isGrabbable)
            {
                return;
            }
            
            IsGrabbed = false;
            OnGrabEnded?.Invoke();
        }

        public void OnUseStart(UsingContext context)
        {
            if (!_isUsable)
            {
                return;
            }
            
            IsUsed = true;
            OnUseStarted?.Invoke();
        }

        public void OnUseEnd()
        {
            if (!_isUsable)
            {
                return;
            }
            
            IsUsed = false;
            OnUseEnded?.Invoke();
        }

        public void OnTouchStart()
        {
            if (!_isTouchable)
            {
                return;
            }
            
            IsTouched = true;
            OnTouchStarted?.Invoke();
        }

        public void OnTouchEnd()
        {
            if (!_isTouchable)
            {
                return;
                
            }
            IsTouched = false;
            OnTouchEnded?.Invoke();
        }

        public void SetIsGrabbable(bool state)
        {
            _isGrabbable = state;
        }
        
        public void SetIsUsable(bool state)
        {
            _isUsable= state;
        }
        
        public void SetIsTouchable(bool state)
        {
            _isTouchable = state;
        }
    }
}
