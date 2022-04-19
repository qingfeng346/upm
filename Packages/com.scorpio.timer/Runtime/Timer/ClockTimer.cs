namespace Scorpio.Timer {
    public class ClockTimer : Timer {
        public ClockTimer(TimerDelegate callBack) : this(callBack, false) { }
        public ClockTimer(TimerDelegate callBack, bool isLoop) : base(TimerType.Clock, callBack, isLoop) { }
        public override long NowTime => TimerManager.Instance.ClockTime;
        public override long NowTimeImmediate => TimerManager.Instance.ClockTimeImmediate;
    }
}