using NLog;

namespace Varwin.UI.ObjectManager
{
    public class UISetParentButton : UIButton
    {

        public ParentCommand ParentCommand;
        
        public override void OnClick()
        {
            UIObject uiObject = gameObject.transform.parent.gameObject.GetComponent<UIObject>();
            ObjectController type = uiObject.GetBaseType();

            ParentManager.Instance.SetSelectedBaseType(type);
            ParentManager.Instance.ParentCommand = ParentCommand;

            LogManager.GetCurrentClassLogger().Info($"Set parent command to {ParentCommand} for {type.Name}");
        }
        
    }

    
}
