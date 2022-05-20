using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scorpio.Debugger {
    public class ConsoleCommands : MonoBehaviour {
        public Transform items;
        public GameObject itemPrefab;
        void Awake() {
            ScorpioDebugger.Instance.addCommandEntry += AddCommandEntry;
            ScorpioDebugger.Instance.CommandEntries.ForEach((commandEntry) => AddCommandEntry(commandEntry) );
        }
        void AddCommandEntry(CommandEntry commandEntry) {
            var item = GameObject.Instantiate<GameObject>(itemPrefab).GetComponent<ConsoleCommandItem>();
            item.transform.SetParent(items);
            item.transform.localScale = Vector3.one;
            item.SetCommandEntry(commandEntry);
        }
        public void OnClickCollider() {
            gameObject.SetActive(false);
        }
    }
}
