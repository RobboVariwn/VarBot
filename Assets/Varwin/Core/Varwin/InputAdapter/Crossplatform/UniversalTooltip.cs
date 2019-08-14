using UnityEngine;

namespace Varwin.VRInput
{
    public class UniversalTooltip : Tooltip, IInitializable<TooltipObject>
    {
        private TooltipObject _tooltip;

        public override Transform drawLineTo
        {
            set => _tooltip.drawLineTo = value;
        }

        public override string displayText
        {
            set => _tooltip.displayText = value;
        }

        public override void ResetTooltip()
        {
            _tooltip.ResetTooltip();
        }

        public void Init(TooltipObject monoBehaviour)
        {
            _tooltip = monoBehaviour;
        }
    }
}
