using UnityEngine;
namespace Scorpio.Unity.Command {
    [CreateAssetMenu(menuName = "Scorpio/CommandSetting")]
    public class CommandSetting : ScriptableObject {
        public string command;
        public string result;
        public string time;
        public string args;
    }
}