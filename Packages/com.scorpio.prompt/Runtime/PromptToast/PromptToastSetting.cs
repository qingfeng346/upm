using UnityEngine;
namespace Scorpio.Prompt {
    [CreateAssetMenu(menuName = "Scorpio/PromptToastSetting")]
    public class PromptToastSetting : ScriptableObject {
        [Header("预制体")]
        public GameObject prefab;
        [Header("显示时长")]
        public float life = 3;
    }
}
