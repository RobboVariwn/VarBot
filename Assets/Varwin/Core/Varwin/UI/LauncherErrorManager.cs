using System;
using SmartLocalization;
using SmartLocalization.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Varwin.Data;

namespace Varwin.UI
{
    // ReSharper disable once InconsistentNaming
    public class LauncherErrorManager : MonoBehaviour
    {
        public GameObject Panel;
        public GameObject LoadAnim;
        public Text Message;
        public TMP_Text Feedback;
        public LocalizedText TryAgainOrSend;
        private bool fatalError = false;
        private Action _reTry;
        public static LauncherErrorManager Instance;
        public TMP_Text LicensedTo;
        private string _stackTrace;

        public UnityEvent showedError;

        private string _lastErrorLocalizedKey;

        private void Awake()
        {
            Instance = this;
            Panel.SetActive(false);
            
            LanguageManager.Instance.OnChangeLanguage += OnChangeLanguage;
        }

        public void Show(string message, Action retry)
        {
            Feedback.text = "";
            Message.text = message;
            _reTry = retry;
            Panel.SetActive(true);
            LoadAnim.SetActive(false);
            
            showedError?.Invoke();
        }

        public void ShowFatal(string message, string stackTrace)
        {
            Feedback.text = "";
            Message.text = message;
            Message.color = Color.red;
            TryAgainOrSend.localizedKey = "SEND_REPORT";
            fatalError = true;
            _stackTrace = stackTrace;
            //ToDo retry rename to send and action change to send
            _reTry = Application.Quit;
            Panel.SetActive(true);
            LoadAnim.SetActive(false);
            
            showedError?.Invoke();
        }

        public void ShowFatalErrorKey(string errorLocalizedKey, string stackTrace)
        {
            _lastErrorLocalizedKey = errorLocalizedKey;
            ShowFatal(LanguageManager.Instance.GetTextValue(_lastErrorLocalizedKey), stackTrace);
        }

        public void Hide()
        {
            Panel.SetActive(false);
            LoadAnim.SetActive(true);
            _lastErrorLocalizedKey = null;
        }

        public void ReTryOrSenfReport()
        {
            _reTry?.Invoke();
            if (fatalError)
            {
                Hide();
                SendReport();
            }
        }

        private void SendReport()
        {
             
        }

        public void License(License license)
        {
            string user;

            if (string.IsNullOrEmpty(license.Company))
            {
                user = license.FirstName + " " + license.LastName;
            }
            else
            {
                user = license.Company;
            }
            
            LicensedTo.gameObject.SetActive(true);
            LicensedTo.text = $"Licensed to {user}\n{license.EditionId} Edition";
        }

        public void Exit()
        {
            Application.Quit();
        }

        private void OnChangeLanguage(LanguageManager languageManager)
        {
            if (!string.IsNullOrEmpty(_lastErrorLocalizedKey))
            {
                Message.text = LanguageManager.Instance.GetTextValue(_lastErrorLocalizedKey);
            }
        }
    }
}
