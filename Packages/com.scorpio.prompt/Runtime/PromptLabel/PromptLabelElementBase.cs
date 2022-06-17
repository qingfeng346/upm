using UnityEngine;
using System.Collections;
namespace Scorpio.Prompt {
    public abstract class PromptLabelElementBase : MonoBehaviour {
        public abstract IEnumerator Show(string label);
    }
}
