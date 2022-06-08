using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
namespace Scorpio.Timer {
    /// <summary> 计时器回调 </summary>
    public delegate void TimerDelegate (Timer timer, object args, object fixedArgs);
    public partial class TimerManager {
        public static TimerManager Instance { get; } = new TimerManager ();
        private List<Timer> m_Timers = new List<Timer> (); //当前正在运行的所有计时器
        private HashSet<Timer> m_AddTimers = new HashSet<Timer> (); //要添加的计时器数组 下一帧统一添加
        private HashSet<Timer> m_DelTimers = new HashSet<Timer> (); //要删除的计时器数组 下一帧统一删除
        private object sync = new object (); //线程锁
        private object timeSync = new object (); //线程锁
        public long GameTimeOffset { get; set; } = 0;
        public long RealTimeOffset { get; set; } = 0;
        public long ClockTimeOffset { get; set; } = 0;
        public long WatchTimeOffset { get; set; } = 0;
        public long GameTime { get; private set; } //游戏时间 受 Time.timeScale 影响
        public long RealTime { get; private set; } //真实时间 不受 Time.timeScale 影响
        public long ClockTime { get; private set; } //设备系统时间 受调整设备时间影响
        public long WatchTime { get; private set; } //计时器时间
        public long GameTimeImmediate => Convert.ToInt64 (Time.time * 1000) + GameTimeOffset; //游戏时间 受 Time.timeScale 影响
        public long RealTimeImmediate => Convert.ToInt64 (Time.realtimeSinceStartup * 1000) + RealTimeOffset; //真实时间 不受 Time.timeScale 影响
        public long ClockTimeImmediate => DateTime.UtcNow.Ticks / 10000 + ClockTimeOffset; //设备系统时间 受调整设备时间影响
        public long WatchTimeImmediate => stopWatch.ElapsedMilliseconds + WatchTimeOffset; //计时器时间
        public Stopwatch stopWatch { get; set; } //计时器实例
        public List<Timer> Timers => m_Timers;
        private TimerManager () {
            stopWatch = Stopwatch.StartNew ();
            UpdateNowTime ();
            TimerBehaviour.Initialize ();
        }
        void UpdateNowTime () {
            GameTime = GameTimeImmediate;
            RealTime = RealTimeImmediate;
            ClockTime = ClockTimeImmediate;
            WatchTime = WatchTimeImmediate;
        }
        /// <summary> 普通循环 </summary>
        public void OnUpdate () {
            UpdateNowTime ();
            lock (sync) {
                if (m_DelTimers.Count > 0) {
                    foreach (var timer in m_DelTimers) {
                        lock (timeSync) {
                            m_Timers.Remove (timer);
                        }
                    }
                    m_DelTimers.Clear ();
                }
                if (m_AddTimers.Count > 0) {
                    foreach (var timer in m_AddTimers) {
                        lock (timeSync) {
                            if (!m_Timers.Contains (timer))
                                m_Timers.Add (timer);
                        }
                    }
                    m_AddTimers.Clear ();
                }
            }
            lock (timeSync) {
                var length = m_Timers.Count;
                for (var i = 0; i < length; ++i) {
                    m_Timers[i].OnUpdate ();
                }
            }
        }
        internal void AddTimer (Timer timer) {
            if (timer == null) { return; }
            //此处不判断 m_Timers.Contains(timer) 是因为 如果一个计时器 在Callback回调里 ResetLength  m_Timer 里面还没有删除此计时器
            lock (sync) {
                m_AddTimers.Add (timer);
            }
        }
        internal void DelTimer (Timer timer) {
            if (timer == null) { return; }
            lock (sync) {
                m_DelTimers.Add (timer);
                m_AddTimers.Remove (timer);
            }
        }

        /// <summary> 添加游戏时间计时器 </summary>
        public Timer AddGameTimer (float length, TimerDelegate callBack) {
            return AddGameTimer (length, callBack, null);
        }
        /// <summary> 添加游戏时间计时器 </summary>
        public Timer AddGameTimer (float length, TimerDelegate callBack, object args) {
            return new GameTimer (callBack).ResetLength (length, args);
        }
        /// <summary> 添加真实时间计时器 </summary>
        public Timer AddRealTimer (float length, TimerDelegate callBack) {
            return AddRealTimer (length, callBack, null);
        }
        /// <summary> 添加真实时间计时器 </summary>
        public Timer AddRealTimer (float length, TimerDelegate callBack, object args) {
            return new RealTimer (callBack).ResetLength (length, args);
        }
        /// <summary> 添加本地时钟计时器 </summary>
        public Timer AddClockTimer (float length, TimerDelegate callBack) {
            return AddClockTimer (length, callBack, null);
        }
        /// <summary> 添加真实时间计时器 </summary>
        public Timer AddClockTimer (float length, TimerDelegate callBack, object args) {
            return new ClockTimer (callBack).ResetLength (length, args);
        }
        /// <summary> watch计时器 </summary>
        public Timer AddWatchTimer (float length, TimerDelegate callBack) {
            return AddWatchTimer (length, callBack, null);
        }
        /// <summary> watch计时器 </summary>
        public Timer AddWatchTimer (float length, TimerDelegate callBack, object args) {
            return new WatchTimer (callBack).ResetLength (length, args);
        }

        public Timer AddGameTimerMS (long length, TimerDelegate callBack) {
            return AddGameTimerMS (length, callBack, null);
        }
        /// <summary> 添加游戏时间计时器 </summary>
        public Timer AddGameTimerMS (long length, TimerDelegate callBack, object args) {
            return new GameTimer (callBack).ResetLengthMS (length, args);
        }
        /// <summary> 添加真实时间计时器 </summary>
        public Timer AddRealTimerMS (long length, TimerDelegate callBack) {
            return AddRealTimerMS (length, callBack, null);
        }
        /// <summary> 添加真实时间计时器 </summary>
        public Timer AddRealTimerMS (long length, TimerDelegate callBack, object args) {
            return new RealTimer (callBack).ResetLengthMS (length, args);
        }
        /// <summary> 添加本地时钟计时器 </summary>
        public Timer AddClockTimerMS (long length, TimerDelegate callBack) {
            return AddClockTimerMS (length, callBack, null);
        }
        /// <summary> 添加真实时间计时器 </summary>
        public Timer AddClockTimerMS (long length, TimerDelegate callBack, object args) {
            return new ClockTimer (callBack).ResetLengthMS (length, args);
        }
        /// <summary> watch计时器 </summary>
        public Timer AddWatchTimerMS (long length, TimerDelegate callBack) {
            return AddWatchTimerMS (length, callBack, null);
        }
        /// <summary> watch计时器 </summary>
        public Timer AddWatchTimerMS (long length, TimerDelegate callBack, object args) {
            return new WatchTimer (callBack).ResetLengthMS (length, args);
        }
        public void Pause<T> () where T : Timer {
            lock (timeSync) {
                foreach (var timer in m_Timers) {
                    if (timer is T)
                        timer.Pause ();
                }
            }
        }
        /// <summary> 继续计时器 </summary>
        public void Play<T>() where T : Timer {
            lock (timeSync) {
                foreach (var timer in m_Timers) {
                    if (timer is T)
                        timer.Play ();
                }
            }
        }
        /// <summary> 清除计时器 </summary>
        public void Shutdown<T>(Func<Timer, bool> check) where T : Timer {
            lock (timeSync) {
                foreach (var timer in m_Timers) {
                    if (timer is T)
                        timer.Shutdown ();
                }
            }
        }
        /// <summary> 暂停计时器 </summary>
        public void Pause (Func<Timer, bool> check) {
            lock (timeSync) {
                foreach (var timer in m_Timers) {
                    if (check (timer))
                        timer.Pause ();
                }
            }
        }
        /// <summary> 继续计时器 </summary>
        public void Play (Func<Timer, bool> check) {
            lock (timeSync) {
                foreach (var timer in m_Timers) {
                    if (check (timer))
                        timer.Play ();
                }
            }
        }
        /// <summary> 清除计时器 </summary>
        public void Shutdown (Func<Timer, bool> check) {
            lock (timeSync) {
                foreach (var timer in m_Timers) {
                    if (check (timer))
                        timer.Shutdown ();
                }
            }
        }
        /// <summary> 暂停所有计时器 </summary>
        public void Pause () {
            lock (timeSync) {
                foreach (var timer in m_Timers) {
                    timer.Pause ();
                }
            }
        }
        /// <summary> 继续所有计时器 </summary>
        public void Play () {
            lock (timeSync) {
                foreach (var timer in m_Timers) {
                    timer.Play ();
                }
            }
        }
        /// <summary> 清除计时器 </summary>
        public void Shutdown () {
            lock (timeSync) {
                m_AddTimers.Clear ();
                m_DelTimers.Clear ();
                foreach (var timer in m_Timers) {
                    timer.Shutdown ();
                }
            }
        }
    }
}