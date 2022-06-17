using System;
using UnityEngine;
namespace Scorpio.Prompt {
    [CreateAssetMenu(menuName = "Scorpio/PromptLabelSetting")]
    public class PromptLabelSetting : ScriptableObject {
        [Header("PromptLabel")]
        public GameObject prefab;
        [Header("最大显示数量")]
        public int count = 20;
        [Header("显示时长")]
        public float life = 3;
        [Header("多条提示坐标间隔")]
        public float space = 40;
        [Header("每条提示最小时间间隔")]
        public float wait = 0.5f;
    }
}
