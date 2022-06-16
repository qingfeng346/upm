//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;
//using System.Collections.Generic;

//namespace Scorpio.Prompt {
//    public class PromptLabel : MonoBehaviour {
//        public static int HINT_COUNT = 20;                      //同时显示的最大数量
//        public static float HINT_LIFE = 3;                      //提示显示时长
//        public static int HINT_FONT_SIZE = 30;                  //字体大小
//        public static int HINT_START_POSITION = -200;           //起始坐标
//        public static int HINT_SPACE = 35;                      //每条提示的间隔
//        public static float HINT_ANIMATION_LENGTH = 0.1f;       //显示动画时长
//        public static float HINT_WAIT_LENGTH = 0.05f;           //每条提示最小事件间隔
//        public static int SortingOrder = 200;                   //显示层级
//        public static Vector3 START_SCALE = Vector3.zero;       //动画起始大小
//        public static int ResolutionWidght = 1334;              //屏幕宽度
//        public static int ResolutionHeight = 750;               //屏幕高度
//        private static PromptLabel instance;
//        public static PromptLabel Instance { get { return instance; } }
//        private static Font font = null;
//        public class LabelElement : MonoBehaviour {
//            private Text m_Text;
//            void Awake() {
//                m_Text = GetComponent<Text>();
//            }
//            IEnumerator Show(string text) {
//                m_Text.text = text;
//                yield return new WaitForSeconds(HINT_LIFE);
//                instance.RemoveLabel(this);
//            }
//        }
//        public static void Show(string text) {
//            if (instance == null || instance.gameObject == null) {
//                var canvas = ResourceManager.Instance.NewCanvas("PromptLabel", ResolutionWidght, ResolutionHeight, SortingOrder);
//                GameObject.DontDestroyOnLoad(canvas.gameObject);
//                instance = canvas.gameObject.AddComponent<PromptLabel>();
//            }
//            instance.AddLabel(text);
//        }
//        public static void Shutdown() {
//            if (instance != null) EngineUtil.DestroyImmediate(instance.gameObject);
//        }
//        public static void SetFont(Font value) {
//            font = value;
//        }

//        private bool m_Wait = false;
//        //提示队列
//        private Queue<string> m_HintStack = new Queue<string>();
//        //当前提示数组
//        private List<LabelElement> m_Hints = new List<LabelElement>();
//        //缓存的提示
//        private Queue<LabelElement> m_Caches = new Queue<LabelElement>();
//        LabelElement GetLabel() {
//            LabelElement label = null;
//            if (m_Caches.Count > 0) {
//                label = m_Caches.Dequeue();
//            } else {
//                Text text = ResourceManager.Instance.NewText;
//                Shadow shadow = text.gameObject.AddComponent<Shadow>();
//                shadow.effectDistance = new Vector2(3, -3);
//                if (font != null) text.font = font;
//                text.fontSize = HINT_FONT_SIZE;
//                text.color = Color.white;
//                text.alignment = TextAnchor.MiddleCenter;
//                text.rectTransform.sizeDelta = Vector2.zero;
//                text.horizontalOverflow = HorizontalWrapMode.Overflow;
//                text.verticalOverflow = VerticalWrapMode.Overflow;
//                EngineUtil.AddChild(gameObject, text);
//                label = text.gameObject.AddComponent<LabelElement>();
//            }
//            EngineUtil.SetActive(label, true);
//            return label;
//        }
//        void RemoveLabel(LabelElement label) {
//            m_Hints.Remove(label);
//            m_Caches.Enqueue(label);
//            EngineUtil.SetActive(label, false);
//        }
//        void AddLabel(string text) {
//            m_HintStack.Enqueue(text);
//            SendMessage("Check", SendMessageOptions.DontRequireReceiver);
//        }
//        IEnumerator Check() {
//            if (m_Wait || m_HintStack.Count == 0) yield break;
//            m_Wait = true;
//            try {
//                var label = GetLabel();
//                label.SendMessage("Show", m_HintStack.Dequeue());
//                label.transform.localScale = START_SCALE;
//                TweenScale.Begin(label.gameObject, HINT_ANIMATION_LENGTH, Vector3.one);
//                m_Hints.Insert(0, label);
//                if (m_Hints.Count > HINT_COUNT) RemoveLabel(m_Hints[HINT_COUNT]);
//                ResetPosition();
//            } catch (System.Exception ex) {
//                logger.error("PromptLabel.Check is error : " + ex.ToString());
//            }
//            yield return new WaitForSeconds(HINT_WAIT_LENGTH);
//            m_Wait = false;
//            SendMessage("Check", SendMessageOptions.DontRequireReceiver);
//        }
//        void ResetPosition() {
//            int count = m_Hints.Count;
//            for (int i = 0; i < count; ++i)
//                EngineUtil.SetLocalPositionY(m_Hints[i], HINT_START_POSITION + i * HINT_SPACE);
//        }
//    }
//}