using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Scorpio.Debugger {
    public class OptionWindow : MonoBehaviour {
        [Serializable]
        public class Prefab {
            public OptionType type;
            public GameObject prefab;
        }
        public GameObject groupItemPrefab;
        public GameObject items;
        public Prefab[] prefabs;
        private Dictionary<OptionType, GameObject> prefabDic;
        private Dictionary<string, GroupItem> groups;
        void Awake() {
            groups = new Dictionary<string, GroupItem>();
            prefabDic = new Dictionary<OptionType, GameObject>();
            Array.ForEach(prefabs, _ => prefabDic[_.type] = _.prefab);
            ScorpioDebugger.Instance.addOption += AddOption;
            ScorpioDebugger.Instance.OptionEntries.ForEach(_ => AddOption(_.title, _.type, _.value));
        }
        public void AddOption(string title, OptionType type, object value) {
            if (!groups.TryGetValue(title, out var groupItem)) {
                groups[title] = groupItem = GameObject.Instantiate<GameObject>(groupItemPrefab).GetComponent<GroupItem>();
                groupItem.SetTitle(title);
                groupItem.transform.SetParent(items.transform);
                groupItem.transform.localScale = Vector3.one;
            }
            var item = GameObject.Instantiate<GameObject>(prefabDic[type]).GetComponent<OptionItemBase>();
            item.transform.SetParent(groupItem.transform);
            item.transform.localScale = Vector3.one;
            item.SetValue(value);
        }
    }               
}
