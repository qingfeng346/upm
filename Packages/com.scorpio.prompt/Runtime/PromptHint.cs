//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;
//using System.Collections.Generic;
//using Commons.Util;
//public class PromptHint : MonoBehaviour {
//    public static float HINT_LIFE = 3;
//	public static int SortingOrder = 200;
//	private static string font = "";
//    private static PromptHint instance = null;
//    public static void Show(string text) {
//        if (instance == null) {
//            GameObject obj = Instantiate(Resources.Load("Common/PromptHint")) as GameObject;
//            DontDestroyOnLoad(obj);
//			var canvas = obj.GetComponent<Canvas>();
//			canvas.sortingOrder = SortingOrder;
//            instance = obj.AddComponent<PromptHint>();
//        }
//        instance.AddLabel(text);
//    }
//    public class HintElement : MonoBehaviour {
//        private Text m_Text;
//        void Awake() {
//            m_Text = GetComponentInChildren<Text>();
//        }
//        IEnumerator Show(string text) {
//            m_Text.text = text;
//            yield return new WaitForSeconds(HINT_LIFE);
//            instance.RemoveLabel(this);
//        }
//        public void UpdateFont() {
//            ResourceManager.GetInstance().SetFont(m_Text, font);
//        }
//    }
//    public static void SetFont(string value) {
//		font = value;
//    }
//    private GameObject m_Item;
//    private GameObject m_Grid;
//    //当前提示数组
//    private List<HintElement> m_Hints = new List<HintElement>();
//    //缓存的提示
//    private Queue<HintElement> m_Caches = new Queue<HintElement>();
//    void Awake() {
//        m_Grid = EngineUtil.FindChild(gameObject, "Grid");
//        m_Item = EngineUtil.FindChild(gameObject, "Item");
//        EngineUtil.ResetCanvasScaler(GetComponent<CanvasScaler>());
//    }
//    private HintElement GetLabel() {
//        HintElement label = null;
//        if (m_Caches.Count > 0) {
//            label = m_Caches.Dequeue();
//			label.UpdateFont();
//            EngineUtil.SetActive(label, true);
//        } else {
//            GameObject obj = Instantiate(m_Item) as GameObject;
//            EngineUtil.AddChild(m_Grid, obj);
//            EngineUtil.SetActive(obj, true);
//            label = obj.AddComponent<HintElement>();
//            label.UpdateFont();
//        }
//        return label;
//    }
//    public void RemoveLabel(HintElement label) {
//        m_Hints.Remove(label);
//        m_Caches.Enqueue(label);
//        EngineUtil.SetActive(label, false);
//    }
//    public void AddLabel(string text) {
//        try {
//            var label = GetLabel();
//            label.transform.SetAsLastSibling();
//            label.SendMessage("Show", text);
//            m_Hints.Insert(0, label);
//        } catch (System.Exception ex) {
//            logger.error("PromptHint.AddLabel is error : " + ex.ToString());
//        }
//    }
//}
