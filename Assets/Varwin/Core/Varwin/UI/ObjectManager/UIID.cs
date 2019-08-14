using UnityEngine;

namespace Varwin.UI.ObjectManager
{
    // ReSharper disable once InconsistentNaming
    public class UIID : MonoBehaviour
    {
        public TextMesh Text;
        public TextMesh Round;
        private ObjectController _objectController;

        public void Init(Transform parent, ObjectController objectController)
        {
            gameObject.transform.parent = parent;
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            _objectController = objectController;
            _objectController.Uiid = this;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_objectController == null)
                return;

            if (_objectController.Id == 0)
            {
                gameObject.SetActive(false);
            }

            Text.text = _objectController.Id.ToString();
            gameObject.transform.LookAt(GameObjects.Instance.Head);

        }
    }
}
