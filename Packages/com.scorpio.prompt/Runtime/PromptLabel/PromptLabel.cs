using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Scorpio.Prompt {
    public class PromptLabel : MonoBehaviour {
        public GameObject items;
        public GameObject item;
        private bool waiting = false;
        //提示队列
        private Queue<string> labels = new Queue<string>();
        //当前提示数组
        private List<PromptLabelElementBase> hints = new List<PromptLabelElementBase>();
        //缓存的提示
        private Queue<PromptLabelElementBase> caches = new Queue<PromptLabelElementBase>();
        public void Show(string label) {
            labels.Enqueue(label);
            StartCoroutine(Check());
        }
        PromptLabelElementBase GetElement() {
            PromptLabelElementBase element;
            if (caches.Count > 0) {
                element = caches.Dequeue();
            } else {
                element = Instantiate(item).GetComponent<PromptLabelElementBase>();
                element.transform.SetParent(items.transform);
            }
            element.gameObject.SetActive(true);
            return element;
        }
        void RemoveElement(PromptLabelElementBase element) {
            hints.Remove(element);
            caches.Enqueue(element);
            element.gameObject.SetActive(false);
        }
        IEnumerator Check() {
            if (waiting) { yield break; }
            waiting = true;
            var count = PromptManager.Instance.LabelSetting.count;
            var wait = PromptManager.Instance.LabelSetting.wait;
            try {
                while (labels.Count > 0) {
                    var element = GetElement();
                    hints.Insert(0, element);
                    if (hints.Count > count) {
                        hints[count].gameObject.SetActive(false);
                    }
                    ResetPosition();
                    StartCoroutine(WaitElement(element, labels.Dequeue()));
                    yield return new WaitForSeconds(wait);
                }
            } finally {
                waiting = false;
            }
        }
        IEnumerator WaitElement(PromptLabelElementBase element, string label) {
            yield return element.Show(label);
            RemoveElement(element);
        }
        void ResetPosition() {
            int count = hints.Count;
            var space = PromptManager.Instance.LabelSetting.space;
            for (int i = 0; i < count; ++i) {
                hints[i].transform.localPosition = new Vector3(0, i * space);
            }
        }
    }
}