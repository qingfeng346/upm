namespace Scorpio.Timer {
    public class WatchTimer : Timer {
        public WatchTimer(TimerDelegate callBack) : this(callBack, false) { }
        public WatchTimer(TimerDelegate callBack, bool isLoop) : base(callBack, isLoop) { }
        public override long NowTime => TimerManager.Instance.WatchTime;
        public override long NowTimeImmediate => TimerManager.Instance.WatchTimeImmediate;
    }
}