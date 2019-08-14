using SmartLocalization;
using UnityEngine;
using Varwin.Data;
using Varwin.Errors;
using Varwin.UI.VRErrorManager;

namespace Varwin
{
    public static class LicenseInfo
    {
        public static License Value;

        public static bool IsDemo => Value?.EditionId == Edition.Starter;
    }

    public enum Edition
    {
        None, Starter, Professional,  Business
    }

    public class LicenceNotificator : MonoBehaviour
    {
        private const int FirstNotify = 5;
        private const int NextNotify = 15;

        private float _firstNotify;
        private float _nextNotify;

        private bool _firstNotifyIsShown;
        private void Start()
        {
            _firstNotify = FirstNotify * 60;
            _nextNotify = NextNotify * 60;
        }
        private void Update()
        {
            if (!_firstNotifyIsShown)
            {
                _firstNotify -= Time.deltaTime;
            }

            else
            {
                _nextNotify -= Time.deltaTime;
            }

            if (!_firstNotifyIsShown && _firstNotify <= 0)
            {
                VRErrorManager.Instance.Show(ErrorHelper.GetErrorDescByCode(Errors.ErrorCode.NotForCommercialUse));
                _firstNotifyIsShown = true;
            }

            if (_nextNotify <= 0)
            {
                VRErrorManager.Instance.Show(ErrorHelper.GetErrorDescByCode(Errors.ErrorCode.NotForCommercialUse));
                _nextNotify =  NextNotify * 60;
            }
             
        }
    }
}