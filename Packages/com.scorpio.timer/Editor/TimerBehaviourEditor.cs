using Scorpio.Timer;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (TimerBehaviour))]
public class TimerBehaviourEditor : Editor {
    class TimerData {
        public string name = "";
        public bool show = true;
        public int count = 0;
        public int watchCount = 0;
        public int minuteCount = 0;
        public int hourCount = 0;
        public int dayCount = 0;
        public int otherCount = 0;
        public TimerData (string name) {
            this.name = name;
        }
        public void Clear () {
            this.count = this.minuteCount = this.hourCount = this.dayCount = this.otherCount = 0;
        }
        public void Add (Timer timer) {
            this.count += 1;
            if (timer.Length <= 60) {
                minuteCount += 1;
            } else if (timer.Length <= 3600) {
                hourCount += 1;
            } else if (timer.Length <= 86400) {
                dayCount += 1;
            } else {
                otherCount += 1;
            }
        }
        public void Draw () {
            if (show = EditorGUILayout.Foldout (show, $"{name} 计时器 : {count}")) {
                EditorGUILayout.LabelField ("  小于一分钟", minuteCount.ToString ());
                EditorGUILayout.LabelField ("  小于一小时", hourCount.ToString ());
                EditorGUILayout.LabelField ("  小于一天", dayCount.ToString ());
                EditorGUILayout.LabelField ("  大于一天", otherCount.ToString ());
            }
        }
    }
    private TimerData game = new TimerData ("Game"), real = new TimerData ("Real"), clock = new TimerData ("Clock"), watch = new TimerData ("Watch");
    public override void OnInspectorGUI () {
        EditorGUILayout.LabelField ("GameTime", TimerManager.Instance.GameTime.ToString ());
        EditorGUILayout.LabelField ("RealTime", TimerManager.Instance.RealTime.ToString ());
        EditorGUILayout.LabelField ("ClockTime", TimerManager.Instance.ClockTime.ToString ());
        EditorGUILayout.LabelField ("WatchTime", TimerManager.Instance.WatchTime.ToString ());
        var timers = TimerManager.Instance.Timers;
        EditorGUILayout.LabelField ("计时器总数量", timers.Count.ToString ());
        game.Clear ();
        real.Clear ();
        clock.Clear ();
        watch.Clear ();
        foreach (var timer in timers) {
            if (timer is GameTimer) {
                game.Add (timer);
            } else if (timer is RealTimer) {
                real.Add (timer);
            } else if (timer is ClockTimer) {
                clock.Add (timer);
            } else if (timer is WatchTimer) {
                watch.Add (timer);
            }
        }
        game.Draw ();
        real.Draw ();
        clock.Draw ();
        watch.Draw ();
        Repaint ();
    }
}