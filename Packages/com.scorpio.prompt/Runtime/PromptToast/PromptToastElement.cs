using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace Scorpio.Prompt {
    public class PromptToastElement : PromptToastElementBase {
        public Text text;
        public override IEnumerator Show(string label) {
            yield break;
        }
    }
}