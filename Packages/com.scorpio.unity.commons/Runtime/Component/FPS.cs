using UnityEngine;
public class FPS : MonoBehaviour
{
    private float updateInterval = 0.5f;
    private float accum = 0.0f;
    private int frames = 0;
    private float timeleft = 0;
    private float _fps = 0;
    public float fps { get { return _fps; } }
    void Start()
    {
        timeleft = updateInterval;
    }
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 25;
        style.normal.textColor = Color.black;
        string str = "FPS:" + _fps.ToString("f2");
        GUI.Label(new Rect(12, 12, 500, 500), str, style);
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(10, 10, 500, 500), str, style);
    }
    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;
        if (timeleft <= 0) {
            _fps = accum / frames;
            timeleft = updateInterval;
            accum = 0;
            frames = 0;
        }
    }
}

