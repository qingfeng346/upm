using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Scorpio.Debugger {
    public class ConsoleLogInfo : MonoBehaviour {
        public Text text;
        public GameObject buttons;
        private LogEntry logEntry;
        internal void SetLogEntry(LogEntry logEntry) {
            this.logEntry = logEntry;
            this.text.text = $"[{logEntry.logType}]{logEntry.logString}";
        }
        public void OnClickCollider() {
            gameObject.SetActive(false);
        }
    }
}
