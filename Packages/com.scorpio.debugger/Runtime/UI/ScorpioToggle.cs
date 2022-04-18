using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scorpio.Console {
    public class ScorpioToggle : Selectable, IPointerClickHandler, ISubmitHandler, IEventSystemHandler, ICanvasElement {
        public static Color DeactiveColor = new Color(0.31f, 0.31f, 0.31f, 1);      //未选中颜色
        public static Color ActiveColor = new Color(0.5f, 0.5f, 0.5f, 1);           //选中颜色
        public Color activeColor = ActiveColor;
        public Color deactiveColor = DeactiveColor;
        public GameObject[] activeGameObjects;
        public GameObject[] deactiveGameObjects;
        public Action<bool, ScorpioToggle> onValueChanged;
        [SerializeField] private ScorpioToggleGroup m_Group;
        [SerializeField] private bool m_IsOn;

        protected override void OnEnable() {
            base.OnEnable();
            this.SetToggleGroup(this.m_Group, false);
            this.CheckShow();
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.SetToggleGroup(null, false);
        }

        public ScorpioToggleGroup group {
            get { return this.m_Group; }
            set {
                this.m_Group = value;
                if (Application.isPlaying) {
                    this.SetToggleGroup(this.m_Group, true);
                }
            }
        }
        private void SetToggleGroup(ScorpioToggleGroup newGroup, bool setMemberValue) {
            var group = this.m_Group;
            if (this.m_Group != null) {
                this.m_Group.UnregisterToggle(this);
            }
            if (setMemberValue) {
                this.m_Group = newGroup;
            }
            if (newGroup != null && this.IsActive()) {
                newGroup.RegisterToggle(this);
            }
            if (newGroup != null && newGroup != group && this.isOn && this.IsActive()) {
                newGroup.NotifyToggleOn(this);
            }
        }
        public bool isOn {
            get { return this.m_IsOn; }
            set { this.Set(value); }
        }

        private void Set(bool value) { this.Set(value, true); }

        private void Set(bool value, bool sendCallback) {
            if (this.m_IsOn != value) {
                this.m_IsOn = value;
                if (this.m_Group != null && this.IsActive()) {
                    if (this.m_IsOn || (!this.m_Group.AnyTogglesOn() && !this.m_Group.allowSwitchOff)) {
                        this.m_IsOn = true;
                        this.m_Group.NotifyToggleOn(this);
                    }
                }
                CheckShow();
                if (sendCallback) {
                    if (onValueChanged != null) onValueChanged(this.m_IsOn, this);
                }
            }
        }
        private void CheckShow() {
            targetGraphic.color = m_IsOn ? activeColor : deactiveColor;
            if (activeGameObjects != null) {
                Array.ForEach(activeGameObjects, (obj) => obj.SetActive(m_IsOn) );
            }
            if (deactiveGameObjects != null) {
                Array.ForEach(deactiveGameObjects, (obj) => obj.SetActive(!m_IsOn) );
            }
        }
        private void InternalToggle() {
            if (this.IsActive() && this.IsInteractable()) {
                this.isOn = !this.isOn;
                DoStateTransition(currentSelectionState, false);
            }
        }
        public virtual void OnPointerClick(PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Left) {
                this.InternalToggle();
            }
        }
        public virtual void OnSubmit(BaseEventData eventData) {
            this.InternalToggle();
        }

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();
            if (!Application.isPlaying)
                CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }
#endif // if UNITY_EDITOR
        public virtual void Rebuild(CanvasUpdate executing) {
#if UNITY_EDITOR
            CheckShow();
#endif
        }

        public virtual void LayoutComplete() { }

        public virtual void GraphicUpdateComplete() { }
    }
}
