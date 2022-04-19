namespace Scorpio.Timer {
    public class RealTimer : Timer {
        public RealTimer(TimerDelegate callBack) : this(callBack, false) { }
        public RealTimer(TimerDelegate callBack, bool isLoop) : base(TimerType.Real, callBack, isLoop) { }
        public override long NowTime => TimerManager.Instance.RealTime;
        public override long NowTimeImmediate => TimerManager.Instance.RealTimeImmediate;
    }
}