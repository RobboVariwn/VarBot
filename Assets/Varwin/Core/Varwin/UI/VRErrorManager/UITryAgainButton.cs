namespace Varwin.UI.VRErrorManager
{
    public class UITryAgainButton : UIButton
    {
        public override void OnClick()
        {
            VRErrorManager.Instance.ReTryOrSendReport();
        }
    }
}
