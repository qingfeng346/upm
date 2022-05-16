using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scorpio.Debugger {
    public class ConsoleCommands : MonoBehaviour {
        public Transform items;
        public GameObject itemPrefab;
        void Awake() {
            ScorpioDebugger.Instance.addCommandEntry += AddCommandEntry;
        }
        void AddCommandEntry(CommandEntry commandEntry) {
            
        }
        internal void Show() {
            gameObject.SetActive(true);
        }
    }
}
