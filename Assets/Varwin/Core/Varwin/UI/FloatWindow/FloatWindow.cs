using UnityEngine;
using Varwin.VRInput;

namespace Varwin.UI
{
    public abstract class FloatWindow : MonoBehaviour
    {
        private Transform Head
        {
            get
            {
                // TODO Удалить после полной интеграции InputAdapter
                var adapterHead = InputAdapter.Instance?.PlayerController?.Nodes?.Head?.Transform;
                if (adapterHead != null)
                {
                    return adapterHead;
                }
                
                return GameObjects.Instance.Head;
            }
        }

        protected void UpdateWindowPosition()
        {
            if (Head == null)
            {
                return;
            }
            
            float angleDiff = Quaternion.FromToRotation(-transform.forward, Head.forward).eulerAngles.y;
            float heightDiff = transform.position.y - Head.transform.position.y + 0.7f;

            if (angleDiff >= 180)
            {
                angleDiff = angleDiff - 360;
            }

            float angleThreshold = 25;

            if (Mathf.Abs(angleDiff) >= angleThreshold)
            {
                float rotationSpeed = 2 * angleThreshold * Mathf.Sign(angleDiff);

                if (Mathf.Abs(angleDiff) > angleThreshold * 3)
                {
                    rotationSpeed *= 5;
                }

                transform.RotateAround(Head.position, Vector3.up, rotationSpeed * Time.deltaTime);
            }
            
            float heightThreshold = 0.2f;
            
            if (Mathf.Abs(heightDiff) >= heightThreshold)
            {
                float moveSpeed = 0.6f;
                
                if (heightDiff < 0)
                {
                    transform.position += Vector3.up * Time.deltaTime * moveSpeed;
                }
                else
                {
                    transform.position -= Vector3.up * Time.deltaTime * moveSpeed;
                }
               
            }
            
            transform.LookAt(Head);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
        }
        
        protected void SetWindowPosition()
        {
            transform.SetParent(null);

            if (Head == null)
            {
                return;
            }

            Vector3 headXZForward = Head.forward;
            headXZForward.y = 0;
            headXZForward.Normalize();

            transform.position = Head.position + 2 * headXZForward - 0.7f * Vector3.up;

            transform.LookAt(Head);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

            transform.SetParent(Head.parent);
        }
    }
}