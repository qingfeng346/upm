using UnityEngine;
public class TweenScale : UITweener {
	public Vector3 from = Vector3.one;
	public Vector3 to = Vector3.one;
	Transform mTrans;
	void Awake () {
		mTrans = transform;
	}
	public Vector3 scale { 
		get { 
			return mTrans != null ? mTrans.localScale : Vector3.zero;
		} 
		set { 
			if (mTrans != null) mTrans.localScale = value;
		} 
	}
	protected override void OnUpdate (float factor, bool isFinished) { scale = from * (1f - factor) + to * factor; }
	public static TweenScale Begin (UnityEngine.Object go, float duration, Vector3 from, Vector3 to) {
		return Begin_impl (UITweener.Begin<TweenScale> (go, duration), duration, from, to);
	}
	public static TweenScale Begin (UnityEngine.Object go, float duration, Vector3 to) {
		TweenScale comp = UITweener.Begin<TweenScale>(go, duration);
		return Begin_impl (comp, duration, comp.scale, to);
	}
	private static TweenScale Begin_impl (TweenScale comp, float duration, Vector3 from, Vector3 to) {
		comp.scale = from;
		comp.from = from;
		comp.to = to;
		if (duration <= 0f) {
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}
}
