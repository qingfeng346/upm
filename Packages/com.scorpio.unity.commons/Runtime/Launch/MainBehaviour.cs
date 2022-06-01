using UnityEngine;
public class MainBehaviour : MonoBehaviour {
    void Update() {
        Game.Update();
    }
    void OnApplicationPause(bool pause) {
        Game.OnApplicationPause(pause);
    }
    void OnApplicationFocus(bool focus) {
        Game.OnApplicationFocus(focus);
    }
}
