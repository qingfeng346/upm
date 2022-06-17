using UnityEngine;
using System.Collections;
namespace Scorpio.Prompt {
    public abstract class PromptToastElementBase : MonoBehaviour {
        public abstract IEnumerator Show(string label);
    }
}