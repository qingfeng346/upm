namespace Scorpio.Timer {
    public class GameTimer : Timer {
        public GameTimer(TimerDelegate callBack) : this(callBack, false) { }
        public GameTimer(TimerDelegate callBack, bool isLoop) : base(callBack, isLoop) { }
        public override long NowTime => TimerManager.Instance.GameTime;
        public override long NowTimeImmediate => TimerManager.Instance.GameTimeImmediate;
    }
}