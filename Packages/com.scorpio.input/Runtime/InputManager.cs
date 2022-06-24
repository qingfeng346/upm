using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
namespace Scorpio.Input {
    public class InputManager {
        private static readonly List<RaycastResult> RaycastResults = new List<RaycastResult>();
        private static InputManager instance;
        public static InputManager Instance => instance ?? (instance = new InputManager());
        //移动多大距离算移动屏幕
        public static float MinMoveDistance = 20;

        public event Action<Vector2> OnTouchUI;                         //UI按下
        public event Action<Vector2> OnTouchUIEnd;                      //UI抬起
        public event Action<Vector2> OnTouchBegin;                      //屏幕按下
        public event Action<Vector2> OnTouchChanged;                    //屏幕手指改变
        public event Action<Vector2, Vector2, Vector2> OnTouchMove;     //拖动屏幕 移动距离超过MinMoveDistance开始移动
        public event Action<Vector2, bool, bool> OnTouchEnd;            //屏幕抬起
        public event Action<Vector2> OnTouchClick;                      //屏幕点击 屏幕按下一段时间内快速抬起
        public event Action<Vector2, float> OnPreTouchLong;             //长按屏幕事件触发前, 此事件一直触发
        public event Action<Vector2> OnTouchLong;                       //长按屏幕 只触发一次，移动后不触发
        public event Action<float> OnZoom;                              //缩放

        private InputBase input;
        private Vector2 beginTouchPosition = Vector2.zero;
        private Vector2 lastTouchPosition = Vector2.zero;
        private Vector2 lastTouchUIPosition = Vector2.zero;
        private int fingerId = 0; //当前移动屏幕的手指ID
        private bool isTouch = false; //是否已点击屏幕
        private bool isTouchUI = false; //是否已点击UI
        private bool isLongTouch = false; //是否已触发长按事件
        private bool isMoveTouch = false; //是否已触发移动事件
        private bool isForceTouch = true;   //强制穿透UI点击
        private float beginTouchTime = 0; //点击屏幕起始
        public float LongTouchTime = 0.7f; //长按事件时长
        public float ClickTouchTime = 0.5f; //点击事件时长
        public bool IsAnimation { get; set; } //是否正在引导
        public bool IsPause { get; set; } //是否暂停操作
        public InputManager() {
            if (Application.isMobilePlatform) {
                input = new InputTouch();
            } else {
                input = new InputMouse();
            }
        }
        public void Initialize() {
            OnTouchUI = null;
            OnTouchUIEnd = null;
            OnTouchBegin = null;
            OnTouchChanged = null;
            OnTouchMove = null;
            OnTouchEnd = null;
            OnTouchClick = null;
            OnPreTouchLong = null;
            OnTouchLong = null;
            OnZoom = null;
            IsPause = false;
            IsAnimation = false;
            Reset();
        }
        public void ResetForce() {
            Reset();
            isForceTouch = true;
        }
        public void Reset() {
            isTouch = false;
            isTouchUI = false;
            isLongTouch = false;
            isMoveTouch = false;
            isForceTouch = false;
            fingerId = -1;
        }
        public Vector2 MousePosition => UnityEngine.Input.mousePosition;
        //是否点击在UI上 EventSystem.current.IsPointerOverGameObject () 在移动平台上有问题
        public static bool IsPointerOverUIObject(Vector2 screenPos) {
            if (EventSystem.current == null)
                return false;
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = screenPos;
            EventSystem.current.RaycastAll(eventDataCurrentPosition, RaycastResults);
            return RaycastResults.Count > 0;
        }
        public void LateUpdate() {
            if (IsAnimation || IsPause) { return; }
            //获取一个手指点击,如果手指不抬起永远获取的是第一个手指
            if (input.Touch(out var screenPos, out var deltePosition, out var touchId)) {
                if (isTouchUI) {                //点击到UI上了
                    lastTouchUIPosition = screenPos;
                } else if (isTouch) {           //点击到非UI上
                    if (fingerId == touchId) {  //手指没变化
                        if (!isLongTouch && !isMoveTouch) {
                            var last = Time.realtimeSinceStartup - beginTouchTime;
                            if (last > LongTouchTime) {
                                isLongTouch = true;
                                Fire(OnTouchLong, beginTouchPosition);
                            } else {
                                Fire(OnPreTouchLong, beginTouchPosition, last);
                            }
                        }
                        if (isMoveTouch) {
                            if (lastTouchPosition != screenPos) {
                                Fire(OnTouchMove, screenPos, lastTouchPosition, deltePosition);
                                lastTouchPosition = screenPos;
                            }
                        } else {
                            if (Vector2.Distance(lastTouchPosition, screenPos) >= MinMoveDistance) {
                                isMoveTouch = true;
                                Fire(OnTouchMove, screenPos, lastTouchPosition, deltePosition);
                                lastTouchPosition = screenPos;
                            }
                        }
                    } else {                    //换了手指,直接相当于移动操作
                        fingerId = touchId;
                        isMoveTouch = true;
                        isLongTouch = false;
                        beginTouchPosition = screenPos;
                        lastTouchPosition = screenPos;
                        Fire(OnTouchChanged, screenPos);
                    }
                } else if (!isForceTouch && IsPointerOverUIObject(screenPos)) {     //判断点击到UI上
                    isTouchUI = true;
                    lastTouchUIPosition = screenPos;
                    Fire(OnTouchUI, screenPos);
                } else {                                //点击到非UI上
                    isTouch = true;
                    fingerId = touchId;
                    isLongTouch = false;
                    isMoveTouch = false;
                    isForceTouch = false;
                    beginTouchTime = Time.realtimeSinceStartup;
                    beginTouchPosition = screenPos;
                    lastTouchPosition = screenPos;
                    Fire(OnTouchBegin, screenPos);
                }
            } else {
                if (isTouchUI) {
                    Fire(OnTouchUIEnd, lastTouchUIPosition);
                } else if (isTouch) {
                    if (!isMoveTouch && Time.realtimeSinceStartup - beginTouchTime < ClickTouchTime) {
                        Fire(OnTouchClick, lastTouchPosition);
                    }
                    Fire(OnTouchEnd, lastTouchPosition, isMoveTouch, isLongTouch);
                }
                Reset();
            }
            float rate = 0f;
            if (input.TouchZoom(ref rate)) {
                Fire(OnZoom, rate);
            }
        }
        void Fire<T1>(Action<T1> action, T1 t1) {
            try {
                action?.Invoke(t1);
            } catch (Exception e) {
                Debug.LogErrorFormat("Action`1 {0} is error : {1}", action, e.ToString());
            }
        }
        void Fire<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2) {
            try {
                action?.Invoke(t1, t2);
            } catch (Exception e) {
                Debug.LogErrorFormat("Action`2 {0} is error : {1}", action, e.ToString());
            }
        }
        void Fire<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) {
            try {
                action?.Invoke(t1, t2, t3);
            } catch (Exception e) {
                Debug.LogErrorFormat("Action`3 {0} is error : {1}", action, e.ToString());
            }
        }
    }
}