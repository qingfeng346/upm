using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Scorpio.Prompt {
    public class PromptToast : MonoBehaviour {
        public GameObject items;
        public GameObject item;
        private bool waiting = false;
        //提示队列
        private Queue<string> labels = new Queue<string>();
        private PromptToastElementBase element;
        private PromptToastElementBase Element {
            get {
                if (element == null) {
                    element = Instantiate(item).GetComponent<PromptToastElementBase>();
                    element.transform.SetParent(items.transform);
                    element.transform.localPosition = Vector3.zero;
                    element.gameObject.SetActive(true);
                }
                return element;
            }
        }
        public void Show(string text) {
            labels.Enqueue(text);
            StartCoroutine(Check());
        }
        IEnumerator Check() {
            if (waiting) { yield break; }
            waiting = true;
            try {
                Element.gameObject.SetActive(true);
                while (labels.Count > 0) {
                    yield return Element.Show(labels.Dequeue());
                }
                Element.gameObject.SetActive(false);
            } finally {
                waiting = false;
            }
        }
    }
}