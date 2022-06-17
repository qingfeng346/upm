using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace Scorpio.Prompt {
    public class PromptToastElement : PromptToastElementBase {
        public Text text;
        public Animation anima;
        public override IEnumerator Show(string label) {
            text.text = label;
            (transform as RectTransform).sizeDelta = new Vector2(text.preferredWidth + 40, 60);
            anima.Play("show");
            yield return new WaitForSeconds(PromptManager.Instance.ToastSetting.life);
            anima.Play("hide");
            yield return new WaitForSeconds(1);
        }
    }
}