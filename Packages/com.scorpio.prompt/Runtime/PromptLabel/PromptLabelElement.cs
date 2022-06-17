using UnityEngine;
using UnityEngine.UI;
using System.Collections;
namespace Scorpio.Prompt {
    public class PromptLabelElement : PromptLabelElementBase {
        public Text text;
        public Animation anima;
        public override IEnumerator Show(string label) {
            this.text.text = label;
            anima.Play("show");
            yield return new WaitForSeconds(PromptManager.Instance.LabelSetting.life);
            anima.Play("hide");
            yield return new WaitForSeconds(0.5f);
        }
    }
}
