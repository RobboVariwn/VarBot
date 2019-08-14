using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin;
    
using ObjectTooltipSize = Varwin.UI.ObjectTooltip.ObjectTooltipSize;

namespace Varwin.UI
{
    public static class TooltipManager
    {
        private static ControllerTooltipManager _controllerTooltipManager;

        public static bool ControllersTooltipsInitialized;
        
        public static void InitializeControllerTooltips()
        {
            if (_controllerTooltipManager == null)
            {
                _controllerTooltipManager = Object.FindObjectOfType<ControllerTooltipManager>();
            }

            ControllersTooltipsInitialized = (_controllerTooltipManager != null);
        }

        public static bool ShowControllerTooltip(string text, ControllerTooltipManager.TooltipControllers controller, ControllerTooltipManager.TooltipButtons button, bool buttonGlow = true)
        {
            InitializeControllerTooltips();

            if (!ControllersTooltipsInitialized)
            {
                return false;
            }

            if (controller == ControllerTooltipManager.TooltipControllers.Both)
            {
                _controllerTooltipManager.SetTooltip(text, ControllerTooltipManager.TooltipControllers.Left, button, buttonGlow);
                _controllerTooltipManager.SetTooltip(text, ControllerTooltipManager.TooltipControllers.Right, button, buttonGlow);
            }
            else
            {
                _controllerTooltipManager.SetTooltip(text, controller, button, buttonGlow);
            }

            return true;
        }

        public static bool HideControllerTooltip(ControllerTooltipManager.TooltipControllers controller, ControllerTooltipManager.TooltipButtons button)
        {
            InitializeControllerTooltips();

            if (!ControllersTooltipsInitialized)
            {
                return false;
            }

            if (controller == ControllerTooltipManager.TooltipControllers.Both)
            {
                _controllerTooltipManager.HideTooltip(ControllerTooltipManager.TooltipControllers.Left, button);
                _controllerTooltipManager.HideTooltip(ControllerTooltipManager.TooltipControllers.Right, button);
            }
            else
            {
                _controllerTooltipManager.HideTooltip(controller, button);
            }

            return true;
        }
        
        
        public static void SetObjectTooltip(GameObject tooltippedObject, string tooltipText, ObjectTooltipSize tooltipSize = ObjectTooltipSize.Large, float verticalOffset = 0.3f)
        {
            ObjectTooltipManager.SetObjectTooltip(tooltippedObject, tooltipText, tooltipSize, verticalOffset);       
        }

        public static void RemoveObjectTooltip(GameObject tooltippedObject)
        {
            ObjectTooltipManager.RemoveObjectTooltip(tooltippedObject);
        }

        public static void UpdateObjectElements(GameObject tooltippedObject)
        {
            ObjectTooltipManager.UpdateObjectElements(tooltippedObject);
        }

        
    }
}