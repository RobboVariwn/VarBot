using UnityEngine;

namespace Varwin.VRInput
{
    public class ScreenPad : MonoBehaviour
    {
        
        /// <summary>
        /// Offset relative to the center of the screen (0; 0 - is center of screen)
        /// </summary>
        public static Vector2 Input = Vector2.zero;

        private void Update()
        {
            float inputX = UnityEngine.Input.mousePosition.x / Screen.width * 2 - 1;
            float inputY = UnityEngine.Input.mousePosition.y / Screen.height * 2 - 1;
            Input = new Vector2(inputX, inputY);
        }
    }
}