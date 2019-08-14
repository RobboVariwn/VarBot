using SmartLocalization;
using Varwin.Commands;
using Varwin.UI;

namespace Varwin.Onboarding
{
    public class ButtonMoveState : State
    {
        public ButtonMoveState(OnboardingStateMachine tutorialStateMachine) : base(tutorialStateMachine)
        {
        }

        public override void OnUpdate()
        {
            foreach (Command command in CommandsManager.CommandList)
            {
                if (!(command is ModifyCommand))
                {
                    continue;
                }

                var o = command.GetObject();

                if (o == null || o.IdObject != StateMachine.SpawningObjectId)
                {
                    continue;
                }

                StateMachine.ChangeState(new MenuOpeningState(StateMachine, MenuOpeningState.TargetObject.Display));
            }
        }

        public override void OnEnter()
        {
            UIMenu.Instance.OnboardingBlock = true;
            string text = LanguageManager.Instance.GetTextValue("TUTORIAL_POPUP_OBJECT_MOVE");
            text = string.Format(text, LanguageManager.Instance.GetTextValue("BUTTON"));
            //PopupState.Show(text, null, StateMachine.ImageContainer.Grab);

            string moveMe = text; //LanguageManager.Instance.GetTextValue("TUTORIAL_MOVE_ME");

            TooltipManager.SetObjectTooltip(StateMachine.ButtonGameObject,
                moveMe,
                ObjectTooltip.ObjectTooltipSize.Large,
                0.1f);

            string toolTip = LanguageManager.Instance.GetTextValue("TUTORIAL_TOOLTIP_GRIP");

            TooltipManager.ShowControllerTooltip(toolTip,
                ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Grip);
        }

        public override void OnExit()
        {
            UIMenu.Instance.OnboardingBlock = false;
            TooltipManager.RemoveObjectTooltip(StateMachine.ButtonGameObject);

            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Grip);
            //PopupWindowManager.ClosePopup();
        }
    }
}
