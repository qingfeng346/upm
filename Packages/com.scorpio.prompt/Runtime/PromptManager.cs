using UnityEngine;
namespace Scorpio.Prompt {
    public class PromptManager {
        public static PromptManager Instance { get; } = new PromptManager();
        public PromptLabelSetting LabelSetting { get; set; }
        public PromptToastSetting ToastSetting { get; set; }
        private PromptManager() {
            LabelSetting = Resources.Load<PromptLabelSetting>("PromptLabelSetting");
            ToastSetting = Resources.Load<PromptToastSetting>("PromptToastSetting");
        }
        private PromptLabel promptLabel = null;
        private PromptLabel PromptLabel {
            get {
                if (promptLabel == null) {
                    promptLabel = Object.Instantiate(LabelSetting.prefab).GetComponent<PromptLabel>();
                    Object.DontDestroyOnLoad(promptLabel.gameObject);
                }
                return promptLabel;
            }
        }
        private PromptToast promptToast = null;
        private PromptToast PromptToast {
            get {
                if (promptToast == null) {
                    promptToast = Object.Instantiate(ToastSetting.prefab).GetComponent<PromptToast>();
                    Object.DontDestroyOnLoad(promptToast.gameObject);
                }
                return promptToast;
            }
        }
        public void ShowToast(string label) {
            PromptToast.Show(label);
        }
    }
}
