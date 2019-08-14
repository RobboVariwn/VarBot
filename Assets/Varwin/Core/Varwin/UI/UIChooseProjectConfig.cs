using System;
using System.Collections.Generic;
using NLog;
using Varwin.Data.ServerData;
using UnityEngine;
using UnityEngine.UI;

namespace Varwin.UI
{
    public class UIChooseProjectConfig : MonoBehaviour
    {
        [SerializeField]
        private Dropdown _dropdown;
        [SerializeField]
        private GameObject _panel;

        private Action<int> _loadConfiguration;
 
        public static UIChooseProjectConfig Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void UpdateDropDown(List<string> configs, Action<int> loadConfiguration)
        {
            _dropdown.options = new List<Dropdown.OptionData>();
            _loadConfiguration = loadConfiguration;
            foreach (var config in configs)
            {
                _dropdown.options.Add(new Dropdown.OptionData {text = config});
            }
        }

        public void Show()
        {
            _panel.SetActive(true);
        }

        public void Hide()
        {
            _panel.SetActive(false);
        }

        public void LoadWorldConfig()
        {
            string configurationName = _dropdown.captionText.text;
            int projectConfigurationId = ProjectData.ProjectStructure.ProjectConfigurations.GetId(configurationName);
            LogManager.GetCurrentClassLogger().Info("Choosen project config id = " + projectConfigurationId);
            _loadConfiguration?.Invoke(projectConfigurationId);
            _panel.SetActive(false);
        }
    }
}
