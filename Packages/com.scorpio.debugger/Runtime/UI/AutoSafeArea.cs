using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Scorpio.Debugger {
    public class AutoSafeArea : MonoBehaviour {
        RectTransform rectTransform => (RectTransform)transform;
        public bool width = false;
        public bool height = false;
        IEnumerator Start() {
            ScorpioDebugger.Instance.safeAreaChanged += (safeArea) => {
                Calc();
            };
            yield return new WaitForEndOfFrame();
            Calc();
        }
        void OnEnable() {
            Calc();
        }
        public void Calc() {
            if (!gameObject.activeInHierarchy) { return; }
            var scaler = GetComponentInParent<CanvasScaler>().referenceResolution;
            var area = new Rect(0, 0, Screen.width, Screen.height);
            var safeArea = Screen.safeArea;
            var offsetMin = Vector2.zero;
            var offsetMax = Vector2.zero;
            if (width) {
                offsetMin.x = safeArea.x / area.width * scaler.x;
                offsetMax.x = (safeArea.xMax - area.xMax) / area.width * scaler.x;
            }
            if (height) {
                offsetMin.y = safeArea.y / area.height * scaler.y;
                offsetMax.y = (safeArea.yMax - area.yMax) / area.height * scaler.y;
            }
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }
    }
}
