using SmartLocalization;
using Varwin.Data;
using Varwin.UI;

namespace Varwin.Onboarding
{
    public class DisplaySelectState : State
    {
        public DisplaySelectState(OnboardingStateMachine tutorialStateMachine) : base(tutorialStateMachine)
        {
        }

        private bool _closedMenu = false;

        private bool CheckSelectedObject(ref int objectId)
        {
            PrefabObject selectedObject = GameStateData.GetPrefabData(ProjectData.SelectedObjectIdToSpawn);

            if (selectedObject == null)
            {
                return false;
            }

            objectId = selectedObject.Id;

            return selectedObject.Guid == OnboardingStateMachine.DisplayGuid;
        }


        public override void OnUpdate()
        {
            if (_closedMenu && ProjectData.ObjectsAreLoaded)
            {
                int objectId = 0;

                if (!CheckSelectedObject(ref objectId))
                {
                    StateMachine.ChangeState(new MenuOpeningState(StateMachine, MenuOpeningState.TargetObject.Display));
                }
                else
                {
                    StateMachine.SpawningObjectId = objectId;
                    StateMachine.ChangeState(new DisplaySpawnState(StateMachine));
                }
            }
        }

        public override void OnEnter()
        {
            StateMachine.UiMenu.HighlightMenuItem(OnboardingStateMachine.DisplayGuid);
            string selectObject = LanguageManager.Instance.GetTextValue("TUTORIAL_TOOLTIP_OBJECT_SELECT");
            selectObject = string.Format(selectObject, LanguageManager.Instance.GetTextValue("DISPLAY"));

            TooltipManager.ShowControllerTooltip(selectObject,
                ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Trigger);
            StateMachine.UiMenu.OnMenuClosed += UiMenuOnOnMenuClosedWhenDisplaySpawn;
            PopupWindowManager.ClosePopup();
        }

        public override void OnExit()
        {
            StateMachine.UiMenu.ResetMenuHighlight();
            StateMachine.UiMenu.OnMenuClosed -= UiMenuOnOnMenuClosedWhenDisplaySpawn;

            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Trigger);
        }


        private void UiMenuOnOnMenuClosedWhenDisplaySpawn()
        {
            _closedMenu = true;
        }
    }
}
