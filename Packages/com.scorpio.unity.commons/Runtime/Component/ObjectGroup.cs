using UnityEngine;

public class ObjectGroup : MonoBehaviour {
    [System.Serializable]
    public class Group {
        public int index;
        public GameObject[] gameObjects;
        public void SetActive(bool active) {
            if (gameObjects == null) { return; }
            for (var i = 0; i < gameObjects.Length;++i) {
                EngineUtil.SetActive(gameObjects[i], active);
            }
        }
    }
    public Group[] groups;
    public void Show(int index) {
        if (groups == null) { return; }
        Group group = null;
        for (var i = 0 ; i < groups.Length; ++i) {
            if (groups[i].index == index) { 
                group = groups[i];
            } else {
                groups[i].SetActive(false);
            }
        }
        if (group != null) { group.SetActive(true); }
    }
}
