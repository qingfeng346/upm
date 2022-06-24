using UnityEngine;
namespace Scorpio.Input {
    public class InputMouse : InputBase {
        private bool pressed = false;
        private Vector2 lastPosition = Vector2.zero;
        public bool Touch(out Vector2 position, out Vector2 deltePosition, out int fingerId) {
            if (UnityEngine.Input.GetMouseButton(0)) {
                fingerId = 0;
                position = UnityEngine.Input.mousePosition;
                if (pressed) {
                    deltePosition = position - lastPosition;
                    lastPosition = position;
                } else {
                    pressed = true;
                    lastPosition = UnityEngine.Input.mousePosition;
                    deltePosition = Vector2.zero;
                }
                return true;
            }
            position = Vector2.zero;
            deltePosition = Vector2.zero;
            fingerId = 0;
            pressed = false;
            return false;
        }
        public bool TouchZoom(ref float rate) {
            if (UnityEngine.Input.GetAxis("Mouse ScrollWheel") < 0) {
                rate = 1f / 0.9f;
                return true;
            }
            if (UnityEngine.Input.GetAxis("Mouse ScrollWheel") > 0) {
                rate = 0.9f;
                return true;
            }
            return false;
        }
    }
}