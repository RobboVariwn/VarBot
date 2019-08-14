namespace Varwin.UI
{
    public class UIDeleteButton : UIButton
    {
        private ObjectController _objectController;

        public void Init(ObjectController newObjectController)
        {
            _objectController = newObjectController;
        }

        public override void OnClick()
        {
            if (_objectController == null || _objectController.RootGameObject == null)
            {
                return;
            }
            
            StartCoroutine(ObjectBehaviourWrapperUtils.DeleteAllChildren(_objectController));
        }
    }
}
