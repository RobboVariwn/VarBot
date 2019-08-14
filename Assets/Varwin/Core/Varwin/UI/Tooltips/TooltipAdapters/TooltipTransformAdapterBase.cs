using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Varwin.UI;
using Varwin.VRInput;

public class TooltipTransformAdapterBase : MonoBehaviour
{
    private ControllerTooltipManager.TooltipControllers controllerType;

    public void Init(ControllerTooltipManager.TooltipControllers typeController)
    {
        controllerType = typeController;
    }

    public virtual Transform GetButtonTransform(ControllerInteraction.ControllerElements controllerElement)
    {
        return null;
    }
}
