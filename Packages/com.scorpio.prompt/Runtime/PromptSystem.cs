//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;
//using System.Collections.Generic;

////
//public class PromptSystem : MonoBehaviour {
//    public static int BLACK_WIDTH = 600;
//    public static int BLACK_HEIGHT = 35;
//    public static float POSITION_X = 0;
//    public static float POSITION_Y = -60;
//    public static float SPEED = 30;
//	public static int FONT_SIZE = 25;
//	public static int SortingOrder = 1;
//    private static PromptSystem instance = null;
//	private static string font = "";
//    public class SystemElement : MonoBehaviour {
//        private Text m_Text;
//        private float m_Width;
//        private float m_Limit = 0;
//        void Awake() {
//            m_Text = GetComponent<Text>();
//			EngineUtil.SetPivot (m_Text, new Vector2 (0, 0.5f));
//        }
//        public float Show(string text) {
//            m_Text.rectTransform.anchoredPosition = Vector2.zero;
//            m_Text.text = text;
//            m_Width = m_Text.preferredWidth;
//            m_Limit = -(m_Width + BLACK_WIDTH);
//            return (m_Width + 50) / SPEED;
//        }
//		public void UpdateFont() {
//			ResourceManager.GetInstance ().SetFont (m_Text, font);
//		}
//        void Update() {
//            Vector2 pos = m_Text.rectTransform.anchoredPosition;
//            pos.x -= SPEED * Time.deltaTime;
//            m_Text.rectTransform.anchoredPosition = pos;
//            if (pos.x < m_Limit - 10) instance.RemoveLabel(this);
//        }
//    }
//    public static void Show(string text) {
//		if (instance == null || instance.gameObject == null) {
//            GameObject obj = ResourceManager.GetInstance().NewCanvas("PromptSystem");
//			var canvas = obj.GetComponent<Canvas>();
//			canvas.sortingOrder = SortingOrder;
//			EngineUtil.AddChild(GameManager.GlobalGameObject, obj);
//            obj.GetComponent<GraphicRaycaster>().enabled = false;
//            instance = obj.AddComponent<PromptSystem>();
//        }
//        EngineUtil.SetActive(instance, true);
//        instance.AddLabel(text);
//    }
//	public static void Shutdown() {
//		if (instance != null) EngineUtil.DestroyImmediate (instance.gameObject);
//	}
//	public static PromptSystem GetInstance() {
//		return instance;
//	}
//	public static void SetFont(string value) {
//		font = value;
//	}
//    private Image m_Panel = null;
//    private bool m_Wait = false;
//    //提示队列
//    private Queue<string> m_HintStack = new Queue<string>();
//    //当前提示数组
//    private List<SystemElement> m_Hints = new List<SystemElement>();
//    //缓存的提示
//    private Queue<SystemElement> m_Caches = new Queue<SystemElement>();
//    void Awake() {
//        m_Panel = ResourceManager.GetInstance().NewImage();
//        m_Panel.color = new Color(0, 0, 0, 0.4f);
//        m_Panel.gameObject.AddComponent<Mask>();
//        EngineUtil.AddChild(gameObject, m_Panel);
//        m_Panel.rectTransform.anchorMin = new Vector2(0.5f, 1);
//        m_Panel.rectTransform.anchorMax = new Vector2(0.5f, 1);
//        m_Panel.rectTransform.sizeDelta = new Vector2(BLACK_WIDTH, BLACK_HEIGHT);
//        m_Panel.rectTransform.anchoredPosition = new Vector2(POSITION_X, POSITION_Y);
//    }
//    private SystemElement GetLabel() {
//        SystemElement label = null;
//        if (m_Caches.Count > 0) {
//            label = m_Caches.Dequeue();
//			label.UpdateFont();
//        } else {
//            Text text = ResourceManager.GetInstance().NewText();
//			text.fontSize = FONT_SIZE;
//            text.color = Color.white;
//            text.rectTransform.anchorMin = new Vector2(1, 0.5f);
//            text.rectTransform.anchorMax = new Vector2(1, 0.5f);
//            text.horizontalOverflow = HorizontalWrapMode.Overflow;
//			text.verticalOverflow = VerticalWrapMode.Overflow;
//            text.alignment = TextAnchor.MiddleLeft;
//            EngineUtil.AddChild(m_Panel, text);
//            label = text.gameObject.AddComponent<SystemElement>();
//			label.UpdateFont();
//        }
//        EngineUtil.SetActive(label, true);
//        return label;
//    }
//    private void RemoveLabel(SystemElement label) {
//        m_Hints.Remove(label);
//        m_Caches.Enqueue(label);
//        EngineUtil.SetActive(label, false);
//        if (m_Hints.Count == 0) EngineUtil.SetActive(gameObject, false);
//    }
//    public void AddLabel(string text) {
//        m_HintStack.Enqueue(text);
//        SendMessage("Check", SendMessageOptions.DontRequireReceiver);
//    }
//    private IEnumerator Check() {
//        if (m_Wait || m_HintStack.Count == 0) yield break;
//        m_Wait = true;
//        float waitTime = 0;
//        try {
//            var label = GetLabel();
//            waitTime = label.Show(m_HintStack.Dequeue());
//            m_Hints.Insert(0, label);
//        } catch (System.Exception ex) {
//            logger.error("PromptSystem.Check is error : " + ex.ToString());
//        }
//        yield return new WaitForSeconds(waitTime);
//        m_Wait = false;
//        SendMessage("Check", SendMessageOptions.DontRequireReceiver);
//    }
//}
