namespace AnimationOrTween {
    public delegate void OnSample(float value, bool isFinished);
    public delegate void OnFinished<T> (T arg1);
	public enum Direction {
		Reverse = -1,
		Toggle = 0,
		Forward = 1,
	}
	public enum EnableCondition {
		DoNothing = 0,
		EnableThenPlay,
	}
	public enum DisableCondition {
		DisableAfterReverse = -1,
		DoNotDisable = 0,
		DisableAfterForward = 1,
	}
	public enum Method {
		Linear,
		EaseIn,
		EaseOut,
		EaseInOut,
		BounceIn,
		BounceOut,
	}
	public enum Style {
		Once,
		Loop,
		PingPong,
	}
}
