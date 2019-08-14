using UnityEngine;
using Valve.VR;
using Varwin.VRInput;

public class SteamVRTooltipAdapter : TooltipTransformAdapterBase
{
    public override Transform GetButtonTransform(ControllerInteraction.ControllerElements controllerElement)
    {
        Transform returnedTransform = null;

        var hintsController = gameObject.GetComponentInChildren<SteamVR_RenderModel>();

        if (!hintsController) return returnedTransform;

        hintsController.gameObject.SetActive(true);
        
        switch (controllerElement)
        {
            case ControllerInteraction.ControllerElements.Trigger:
                returnedTransform = hintsController.gameObject.transform.Find("trigger")?.transform.Find("attach");
                break;
            case ControllerInteraction.ControllerElements.GripLeft:
                returnedTransform = hintsController.gameObject.transform.Find("handgrip")?.transform.Find("attach");
                break;
            case ControllerInteraction.ControllerElements.Touchpad:
                returnedTransform = hintsController.gameObject.transform.Find("trackpad")?.transform.Find("attach");
                break;
            case ControllerInteraction.ControllerElements.ButtonOne:
                returnedTransform = hintsController.gameObject.transform.Find("trigger")?.transform.Find("attach");
                break;
            case ControllerInteraction.ControllerElements.ButtonTwo:
                returnedTransform = hintsController.gameObject.transform.Find("menu_button")?.transform.Find("attach");
                break;
            case ControllerInteraction.ControllerElements.StartMenu:
                returnedTransform = hintsController.gameObject.transform.Find("")?.transform.Find("attach");
                break;
            default:
                returnedTransform = hintsController.gameObject.transform.Find("")?.transform.Find("attach");
                break;
        }

        return returnedTransform;
    }
}
