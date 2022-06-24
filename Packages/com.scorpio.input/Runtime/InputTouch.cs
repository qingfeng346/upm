using UnityEngine;
namespace Scorpio.Input {
    public class InputTouch : InputBase {
        private int touchId = -1;           //首手指的touchId
        private float zoomDistance = 0;     //上一帧两手指的距离
        public bool Touch(out Vector2 position, out Vector2 deltePosition, out int fingerId) {
            if (UnityEngine.Input.touchCount > 0) {
                //找到上一帧点击的手指还在不在
                if (this.touchId != -1) {
                    foreach (var touch in UnityEngine.Input.touches) {
                        if (touch.fingerId == this.touchId) {
                            fingerId = this.touchId;
                            position = touch.position;
                            deltePosition = touch.deltaPosition;
                            return true;
                        }
                    }
                }
                //如果第一次点击或者没有找到上次点击的手指 则重新寻找一个手指
                var firstTouch = UnityEngine.Input.touches[0];
                this.touchId = firstTouch.fingerId;
                fingerId = this.touchId;
                position = firstTouch.position;
                deltePosition = firstTouch.deltaPosition;
                return true;
            }
            position = Vector2.zero;
            deltePosition = Vector2.zero;
            fingerId = 0;
            this.touchId = -1;
            return false;
        }
        public bool TouchZoom(ref float rate) {
            if (UnityEngine.Input.touchCount > 1) {
                var distance = Vector2.Distance(UnityEngine.Input.GetTouch(0).position, UnityEngine.Input.GetTouch(1).position);
                if (zoomDistance == 0) {
                    rate = 1f;
                    zoomDistance = distance;
                } else {
                    var new_distance = distance;
                    rate = zoomDistance / new_distance;
                    zoomDistance = new_distance;
                }
                return true;
            } else {
                zoomDistance = 0;
            }
            return false;
        }
    }
}