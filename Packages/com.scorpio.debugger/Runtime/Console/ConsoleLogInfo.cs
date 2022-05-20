using UnityEngine;
using UnityEngine.UI;
namespace Scorpio.Debugger {
    public class ConsoleLogInfo : MonoBehaviour {
        public Text text;
        public Transform buttons;
        public GameObject itemPrefab;
        private LogEntry logEntry;
        void Awake() {
            ScorpioDebugger.Instance.addLogOperation += AddLogOperation;
            ScorpioDebugger.Instance.LogOperationEntries.ForEach((logOperation) => AddLogOperation(logOperation));
        }
        void AddLogOperation(LogOperationEntry logOperation) {
            var item = Instantiate(itemPrefab);
            item.transform.SetParent(buttons);
            item.transform.localScale = Vector3.one;
            item.GetComponentInChildren<Text>().text = logOperation.label;
            item.GetComponent<Button>().onClick.AddListener(() => {
                logOperation.action?.Invoke(logEntry);
            });
        }
        internal void SetLogEntry(LogEntry logEntry) {
            this.logEntry = logEntry;
            this.text.text = $"[{logEntry.logType}]{logEntry.logString}";
        }
        public void OnClickCollider() {
            gameObject.SetActive(false);
        }
    }
}
