using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Scorpio.Debugger {
    public class AutoSafeArea : MonoBehaviour {
        RectTransform rectTransform => (RectTransform)transform;
        IEnumerator Start() {
            ScorpioDebugger.Instance.safeAreaChanged += (safeArea) => {
                StartCoroutine(CalcSize());
            };
            yield return CalcSize();
        }
        IEnumerator CalcSize() {
            yield return new WaitForEndOfFrame();
            var scaler = GetComponentInParent<CanvasScaler>().referenceResolution;
            var area = new Rect(0, 0, Screen.width, Screen.height);
            var safeArea = Screen.safeArea;
            rectTransform.offsetMin = new Vector2(safeArea.x / area.width * scaler.x, safeArea.y / area.height * scaler.y);
            rectTransform.offsetMax = new Vector2((safeArea.xMax - area.xMax) / area.width * scaler.x, (safeArea.yMax - area.yMax) / area.height * scaler.y);
        }
    }
}
