using UnityEngine;
public class TweenTransform : UITweener {
	public Vector3 fromPosition;
	public Transform fromTransform;
	public Transform to;

	Transform mTrans;
	void Awake () {
		mTrans = transform;
	}
	protected void Restart() {
		if (mTrans == null) { mTrans = transform; }
		fromPosition = mTrans.position;
	}
	protected override void OnUpdate (float factor, bool isFinished) {
		if (to != null) {
			if (fromTransform != null) {
				mTrans.position = fromTransform.position * (1f - factor) + to.position * factor;
			} else {
				mTrans.position = fromPosition * (1f - factor) + to.position * factor;
			}
		}
	}
	static public TweenTransform Begin (UnityEngine.Object go, float duration, Transform to) { return Begin(go, duration, null, to); }
	static public TweenTransform Begin (UnityEngine.Object go, float duration, Transform from, Transform to) {
		TweenTransform comp = UITweener.Begin<TweenTransform>(go, duration);
		comp.fromTransform = from;
		comp.to = to;
		comp.Restart();
		if (duration <= 0f) {
			comp.Sample(1f, true);
			comp.enabled = false;
		} else {
			comp.Reset();
		}
		return comp;
	}
}
