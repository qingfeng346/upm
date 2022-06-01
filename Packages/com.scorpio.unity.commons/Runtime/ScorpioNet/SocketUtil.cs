using UnityEngine;
namespace Scorpio.Net {
    public class SocketBehaviour : MonoBehaviour {
        public ScorpioSocket socket;
        void FixedUpdate() {
            socket.ProcessOpMessage();
        }
    }
    public static class SocketUtil {
        public static GameObject RegisterSocketBehaviour(ScorpioSocket socket) {
            var obj = Game.NewGameObject("__Socket");
            obj.AddComponent<SocketBehaviour>().socket = socket;
            return obj;
        }
    }
}
