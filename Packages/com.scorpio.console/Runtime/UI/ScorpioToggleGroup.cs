using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scorpio.Console {
    public class ScorpioToggleGroup : UIBehaviour {
        private List<ScorpioToggle> m_Toggles = new List<ScorpioToggle>();
        [SerializeField] private bool m_AllowSwitchOff = false;
        public bool allowSwitchOff {
            get { return m_AllowSwitchOff; }
            set { m_AllowSwitchOff = value; }
        }
        private void ValidateToggleIsInGroup(ScorpioToggle toggle) {
            if (toggle == null || !this.m_Toggles.Contains(toggle)) {
                throw new ArgumentException(string.Format("Toggle {0} is not part of ToggleGroup {1}", new object[] {
                toggle,
                this
            }));
            }
        }
        public void NotifyToggleOn(ScorpioToggle toggle) {
            this.ValidateToggleIsInGroup(toggle);
            for (int i = 0; i < this.m_Toggles.Count; i++) {
                if (!(this.m_Toggles[i] == toggle)) {
                    this.m_Toggles[i].isOn = false;
                }
            }
        }

        public void UnregisterToggle(ScorpioToggle toggle) {
            this.m_Toggles.Remove(toggle);
        }

        public void RegisterToggle(ScorpioToggle toggle) {
            if (!this.m_Toggles.Contains(toggle)) {
                this.m_Toggles.Add(toggle);
            }
        }

        public bool AnyTogglesOn() {
            return this.m_Toggles.Find((x) => x.isOn) != null;
        }

        public IEnumerable<ScorpioToggle> ActiveToggles() {
            return from x in this.m_Toggles where x.isOn select x;
        }

        public void SetAllTogglesOff() {
            var allowSwitchOff = this.allowSwitchOff;
            this.allowSwitchOff = true;
            for (int i = 0; i < this.m_Toggles.Count; i++) {
                this.m_Toggles[i].isOn = false;
            }
            this.allowSwitchOff = allowSwitchOff;
        }
    }
}