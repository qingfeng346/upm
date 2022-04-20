using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Scorpio.Debugger {
    public class DebugLogListView : MonoBehaviour {
        private const float ItemHeight = 40;
        private static readonly Vector2 ItemSize = new Vector2(0, ItemHeight);
        public ScrollRect viewportTransform;            //拖拽窗口
        public GameObject debugLogItem;                 //Log Item
        public GameObject buttonBottom;                 //到底部按钮
        private float viewportHeight;                   //窗口高度
        private bool autoToBottom;                      //是否自动到底部
        private List<ConsoleLogItem> items = new List<ConsoleLogItem>();        //所有日志元素
        private List<LogEntry> entrys = new List<LogEntry>();     //要显示的日志
        private int minNum = 0;
        private int maxNum = 0;
        private RectTransform rectTransform { get { return transform as RectTransform; } }
        void Awake() {
            viewportHeight = 600 - 72;
            viewportTransform.onValueChanged.AddListener(this.OnValueChanged);
            ScorpioDebugUtil.RegisterClick(buttonBottom, () => { SetAutoToBottom(true); });
            SetAutoToBottom(true);
        }
        void OnValueChanged(Vector2 pos) {
            if (autoToBottom && pos.y > 0.01f) {
                SetAutoToBottom(false);
            }
            this.UpdateLogs(false);
        }
        void SetAutoToBottom(bool enable) {
            if (enable) {
                autoToBottom = true;
                buttonBottom.SetActive(false);
                viewportTransform.normalizedPosition = Vector2.zero;
            } else {
                autoToBottom = false;
                buttonBottom.SetActive(true);
            }
        }
        public void SetLogEntrys(List<LogEntry> entrys) {
            this.entrys = entrys;
            UpdateLogs(true);
        }
        public void AddLogEntry(LogEntry entry) {
            this.entrys.Add(entry);
            UpdateLogs(false);
        }
        public void UpdateLogs(bool force) {
            var y = rectTransform.anchoredPosition.y;
            var minNum = Mathf.Clamp(Mathf.FloorToInt(y / ItemHeight), 0, entrys.Count);
            var maxNum = Mathf.Clamp(Mathf.FloorToInt((y + viewportHeight) / ItemHeight) + 1, 0, entrys.Count);
            rectTransform.sizeDelta = new Vector2( 0f, entrys.Count * ItemHeight );
            if (minNum == this.minNum && maxNum == this.maxNum && !force) { return; }
            this.minNum = minNum;
            this.maxNum = maxNum;
            foreach (var item in items) { item.gameObject.SetActive(false); }
            var itemNum = 0;
            for (var i = minNum; i < maxNum; ++i) {
                ConsoleLogItem item;
                if (itemNum >= items.Count) {
                    item = GameObject.Instantiate(debugLogItem).GetComponent<ConsoleLogItem>();
                    item.SetParent(transform);
                    item.rectTransform.sizeDelta = ItemSize;
                    items.Add(item);
                    itemNum++;
                } else {
                    item = items[itemNum++];
                    item.gameObject.SetActive(true);
                }
                item.gameObject.name = i.ToString();
                item.rectTransform.anchoredPosition = new Vector2(0, -i * ItemHeight);
                item.SetLogEntry(entrys[i]);
            }
            if (autoToBottom) { viewportTransform.normalizedPosition = Vector2.zero; }
        }
    }

}
