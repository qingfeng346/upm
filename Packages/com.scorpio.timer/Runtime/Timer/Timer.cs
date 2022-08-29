using System;
using UnityEngine;
namespace Scorpio.Timer {
    public abstract class Timer {
        /// <summary> 计时器回调 </summary>
        public TimerDelegate CallBack { get; set; }
        /// <summary> 计时器是否已经结束 </summary>
        public bool IsOver { get; private set; }
        /// <summary> 计时器是否暂停 </summary>
        public bool IsPause { get; private set; }
        /// <summary> 是否是循环计时器 </summary>
        public bool IsLoop { get; private set; }
        /// <summary> 计时器参数 固定参数 </summary>
        public object FixedArgs { get; set; }
        /// <summary> 计时器参数, ResetLength 时设置 </summary>
        public object Args { get; set; }
        
        private long m_StartTime; //计时器开始时间
        private long m_PauseTime; //计时器暂停的时间
        private long m_PassTime;

        protected Timer (TimerDelegate callBack, bool isLoop) {
            this.CallBack = callBack;
            this.IsPause = false;
            this.IsOver = true;
            this.IsLoop = isLoop;
        }
        //当前时间
        public abstract long NowTime { get; }
        //当前时间
        public abstract long NowTimeImmediate { get; }

        /// <summary> 重置计时器时间 可以设置剩余时间 </summary>
        public Timer ResetLastLength (float length, float lastTime) {
            return ResetLastLength (length, lastTime, null);
        }
        /// <summary> 重置计时器时间 可以设置剩余时间 </summary>
        public Timer ResetLastLength (float length, float lastTime, object args) {
            return ResetPassLength (length, length - lastTime, args);
        }
        /// <summary> 重置计时器时间 可以设置已过时间 </summary>
        public Timer ResetPassLength (float length, float passTime) {
            return ResetPassLength (length, passTime, null);
        }
        /// <summary> 重置计时器时间 可以设置已过时间 </summary>
        /// <param name="length">总时长</param>
        /// <param name="passTime">已过的时长</param>
        /// <param name="args">参数</param>
        public Timer ResetPassLength (float length, float passTime, object args) {
            return ResetPassLengthMS(Convert.ToInt64(length * 1000), Convert.ToInt64(passTime * 1000), args);
        }
        /// <summary> 重置计时器时间 如果时间小于等于0 则不调用 </summary>
        public Timer ResetCheckLength (float length) {
            return ResetCheckLength (length, null);
        }
        /// <summary> 重置计时器时间 如果时间小于等于0 则不调用 </summary>
        public Timer ResetCheckLength (float length, object args) {
            if (length <= 0) return this;
            return ResetLength (length, args);
        }
        /// <summary> 重置计时器时间 不检测时间长度 </summary>
        public Timer ResetLength (float length) {
            return ResetLength (length, null);
        }
        /// <summary> 重置计时器时间 不检测时间长度 </summary>
        public Timer ResetLength (float length, object args) {
            return ResetLengthMS(Convert.ToInt64(length * 1000), args);
        }



        /// <summary> 重置计时器时间 可以设置剩余时间 </summary>
        public Timer ResetLastLengthMS (long length, long lastTime) {
            return ResetLastLengthMS (length, lastTime, null);
        }
        /// <summary> 重置计时器时间 可以设置剩余时间 </summary>
        public Timer ResetLastLengthMS (long length, long lastTime, object args) {
            return ResetPassLengthMS (length, length - lastTime, args);
        }
        /// <summary> 重置计时器时间 可以设置已过时间 </summary>
        public Timer ResetPassLengthMS(long length, long passTime) {
            return ResetPassLengthMS(length, passTime, null);
        }
        /// <summary> 重置计时器时间 可以设置已过时间 </summary>
        /// <param name="length">总时长</param>
        /// <param name="passTime">已过的时长</param>
        /// <param name="args">参数</param>
        public Timer ResetPassLengthMS(long length, long passTime, object args) {
            m_PassTime = passTime;
            m_StartTime = NowTimeImmediate - m_PassTime;
            LengthMS = length;
            Args = args;
            IsPause = false;
            IsOver = false;
            TimerManager.Instance.AddTimer (this);
            return this;
        }
        public Timer ResetCheckLengthMS (long length) {
            return ResetCheckLengthMS (length, null);
        }
        /// <summary> 重置计时器时间 如果时间小于等于0 则不调用 </summary>
        public Timer ResetCheckLengthMS (long length, object args) {
            if (length <= 0) return this;
            return ResetLengthMS (length, args);
        }
        /// <summary> 重置计时器时间 不检测时间长度 </summary>
        public Timer ResetLengthMS(long length) {
            return ResetLengthMS(length, null);
        }
        /// <summary> 重置计时器时间 不检测时间长度 </summary>
        public Timer ResetLengthMS(long length, object args) {
            m_PassTime = 0;
            m_StartTime = NowTimeImmediate;
            LengthMS = length;
            Args = args;
            IsPause = false;
            IsOver = false;
            TimerManager.Instance.AddTimer (this);
            return this;
        }

        /// <summary> 删除计时器 </summary>
        public void Shutdown () {
            if (IsOver) { return; }
            IsOver = true;
            TimerManager.Instance.DelTimer (this);
        }
        /// <summary> 暂停计时器 </summary>
        public void Pause () {
            if (IsPause == false) {
                IsPause = true;
                m_PauseTime = NowTime;
            }
        }
        /// <summary> 计时器继续 </summary>
        public void Play () {
            if (IsPause == true) {
                IsPause = false;
                m_StartTime += (NowTime - m_PauseTime);
            }
        }
        /// <summary> 计时器时长 </summary>
        public long LengthMS { get; private set; }
        /// <summary> 剩余时间:毫秒 </summary>
        public long SurplusTimeMS => LengthMS - LastTimeMS;
        /// <summary> 计时器持续时间:毫秒 </summary>
        public long LastTimeMS => IsPause ? m_PauseTime - m_StartTime : NowTime - m_StartTime;
        
        /// <summary> 计时器时长:秒 </summary>
        public float Length => LengthMS / 1000f;
        /// <summary> 剩余时间:秒 </summary>
        public float SurplusTime => SurplusTimeMS / 1000f;
        /// <summary> 剩余时间:秒 </summary>
        public float LastTime => LastTimeMS / 1000f;
        /// <summary> 持续时间百分比 </summary>
        public float LastPercent { get { return LastTimeMS / Convert.ToSingle(Length); } }
        /// <summary> 剩余时间百分比 </summary>
        public float SurplusPercent { get { return SurplusTimeMS / Convert.ToSingle(Length); } }
        private void Call () {
            try {
                if (IsOver) return;
                if (IsLoop) {
                    m_StartTime += LengthMS;
                } else {
                    IsOver = true;
                    TimerManager.Instance.DelTimer (this);
                }
                if (CallBack != null) CallBack(this, Args, FixedArgs);
            } catch (Exception e) {
                if (CallBack != null) {
#if UNITY_EDITOR
                    string target = CallBack.Target != null ? CallBack.Target.GetType ().Name : "";
                    string method = CallBack.Method != null ? CallBack.Method.ToString () : "";
                    Debug.LogError ($"TimeCallBack is error {target} - {method} stack : {e}");
#else
                    Debug.LogError ($"TimeCallBack is error stack : {e}");
#endif
                } else {
                    Debug.LogError ($"TimeCallBack is error : {e}");
                }
            }
        }
        /// <summary> 计时器更新 </summary>
        public void OnUpdate () {
            if (SurplusTimeMS < 0) {
                Call ();
            }
        }
    }
}