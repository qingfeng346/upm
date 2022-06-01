using UnityEngine;
public class TweenPosition : UITweener {
	public Vector3 from;
	public Vector3 to;
	Transform mTrans;
	void Awake () {
		mTrans = transform;
	}
	public Vector3 position { 
		get { 
			return mTrans != null ? mTrans.localPosition : Vector3.zero;
		} 
		set { 
			if (mTrans != null) mTrans.localPosition = value;
		} 
	}
	protected override void OnUpdate (float factor, bool isFinished) { position = from * (1f - factor) + to * factor; }
	public static TweenPosition Begin (UnityEngine.Object go, float duration, Vector3 from, Vector3 to) {
		return Begin_impl (UITweener.Begin<TweenPosition> (go, duration), duration, from, to);
	}
	public static TweenPosition Begin (UnityEngine.Object go, float duration, Vector3 to) {
		TweenPosition comp = UITweener.Begin<TweenPosition>(go, duration);
		return Begin_impl (comp, duration, comp.position, to);
	}
	private static TweenPosition Begin_impl (TweenPosition comp, float duration, Vector3 from, Vector3 to) {
		comp.position = from;
		comp.from = from;
		comp.to = to;
		if (duration <= 0f) {
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}
}
