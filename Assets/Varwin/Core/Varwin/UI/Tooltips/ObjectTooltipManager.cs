using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ObjectTooltipSize = Varwin.UI.ObjectTooltip.ObjectTooltipSize;

namespace Varwin.UI
{
    public static class ObjectTooltipManager
    {
               
        private static List<ObjectTooltip> _objectTooltips;
        
        public static bool SetObjectTooltip(GameObject tooltippedObject, string tooltipText, ObjectTooltipSize objectTooltipSize, float verticalOffset)
        {
            if (tooltippedObject == null)
            {
                return false;
            }
            
            ObjectTooltip tooltip = GetToolTipByGameObject(tooltippedObject);

            if (tooltip == null || tooltip.TooltipSize != objectTooltipSize)
            {
                if (tooltip != null)
                {
                    RemoveObjectTooltip(tooltippedObject);
                }
                
                string tooltipPath;

                switch (objectTooltipSize)
                {
                    case ObjectTooltipSize.Small:
                        tooltipPath = "ObjectTooltipSmall";

                        break;
                    case ObjectTooltipSize.Large:
                        tooltipPath = "ObjectTooltipLarge";

                        break;

                    default:
                        tooltipPath = "ObjectTooltipLarge";

                        break;
                }

                GameObject tooltipObject = Object.Instantiate(Resources.Load(tooltipPath) as GameObject);
                tooltip = tooltipObject.GetComponent<ObjectTooltip>();

                if (_objectTooltips == null)
                {
                    _objectTooltips = new List<ObjectTooltip>();
                }

                _objectTooltips.Add(tooltip);
            }

            tooltip.SetTooltip(tooltippedObject, tooltipText, objectTooltipSize, verticalOffset);

            return true;
        }

        public static bool RemoveObjectTooltip(GameObject tooltippedObject)
        {
            if (tooltippedObject == null)
            {
                return false;
            }
            ObjectTooltip tooltip = GetToolTipByGameObject(tooltippedObject);

            if (tooltip == null)
            {
                return false;
            }

            _objectTooltips.Remove(tooltip);
            Object.Destroy(tooltip.gameObject);

            return true;
        }
        
        public static void UpdateObjectElements(GameObject tooltippedObject)
        {
            ObjectTooltip tooltip = GetToolTipByGameObject(tooltippedObject);

            if (tooltip == null)
            {
                return;
            }

            tooltip.UpdateObjectElements();
        }

        private static ObjectTooltip GetToolTipByGameObject(GameObject o)
        {
            _objectTooltips?.RemoveAll(x => x == null);
                
            return _objectTooltips?.FirstOrDefault(x => x.TooltippedObject == o);
        }
    }
}