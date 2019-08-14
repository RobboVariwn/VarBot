using SmartLocalization;
using Varwin.UI;

namespace Varwin.Onboarding
{
    public class MenuOpeningState : State
    {
        private bool _conditionsMenuOpening;
        private readonly TargetObject _targetObject;

        public MenuOpeningState(OnboardingStateMachine tutorialStateMachine, TargetObject targetObject) :
            base(tutorialStateMachine) => _targetObject = targetObject;

        public override void OnUpdate()
        {
            if (!_conditionsMenuOpening)
            {
                return;
            }

            switch (_targetObject)
            {
                case TargetObject.Button:
                    StateMachine.ChangeState(new ButtonSelectState(StateMachine));

                    break;
                case TargetObject.Display:
                    StateMachine.ChangeState(new DisplaySelectState(StateMachine));

                    break;
                case TargetObject.Lamp:
                    StateMachine.ChangeState(new LampSelectState(StateMachine));

                    break;
                case TargetObject.Save:
                    StateMachine.ChangeState(new SavingState(StateMachine));

                    break;
                case TargetObject.ViewMode:
                    StateMachine.ChangeState(new MoveToViewModeState(StateMachine));

                    break;
            }
        }

        public override void OnEnter()
        {
            string popupText = string.Format(LanguageManager.Instance.GetTextValue("TUTORIAL_POPUP_OPEN_MENU"),
                GetTargetString(_targetObject));

            PopupHelper.Show(popupText,
                null,
                StateMachine.ImageContainer.Menu);
            string openMenu = LanguageManager.Instance.GetTextValue("TUTORIAL_TOOLTIP_OPEN_MENU");

            TooltipManager.ShowControllerTooltip(openMenu,
                ControllerTooltipManager.TooltipControllers.Left,
                ControllerTooltipManager.TooltipButtons.ButtonTwo);
            StateMachine.UiMenu.OnMenuOpened += () => { _conditionsMenuOpening = true; };
        }

        private string GetTargetString(TargetObject target)
        {
            switch (_targetObject)
            {
                case TargetObject.Button:

                    return LanguageManager.Instance.GetTextValue("ONBOARDING_POPUP_MENU_SELECT_BUTTON");
                case TargetObject.Display:

                    return LanguageManager.Instance.GetTextValue("ONBOARDING_POPUP_MENU_SELECT_DISPLAY");
                case TargetObject.Lamp:

                    return LanguageManager.Instance.GetTextValue("ONBOARDING_POPUP_MENU_SELECT_LAMP");
                case TargetObject.Save:

                    return LanguageManager.Instance.GetTextValue("ONBOARDING_POPUP_MENU_SELECT_SAVE");
                case TargetObject.ViewMode:

                    return LanguageManager.Instance.GetTextValue("ONBOARDING_POPUP_MENU_SELECT_VIEW");

                default: return "";
            }
        }

        public override void OnExit()
        {
            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Left,
                ControllerTooltipManager.TooltipButtons.ButtonTwo);

            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Trigger);
            PopupWindowManager.ClosePopup();
        }

        public enum TargetObject
        {
            Button, Display, Lamp, Save, ViewMode
        }
    }
}
