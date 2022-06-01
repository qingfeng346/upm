using UnityEngine;
public class TweenSizeDelta : UITweener {
	public Vector2 from;
	public Vector2 to;
	RectTransform mTrans;
	void Awake () {
		mTrans = GetComponent<RectTransform>();
	}
	public Vector2 size { 
		get { 
			return mTrans != null ? mTrans.sizeDelta : Vector2.zero;
		} 
		set { 
			if (mTrans != null) mTrans.sizeDelta = value;
		} 
	}
	protected override void OnUpdate (float factor, bool isFinished) { size = from * (1f - factor) + to * factor; }
	public static TweenSizeDelta Begin (UnityEngine.Object go, float duration, Vector2 from, Vector2 to) {
		return Begin_impl (UITweener.Begin<TweenSizeDelta> (go, duration), duration, from, to);
	}
	public static TweenSizeDelta Begin (UnityEngine.Object go, float duration, Vector2 to) {
		TweenSizeDelta comp = UITweener.Begin<TweenSizeDelta>(go, duration);
		return Begin_impl (comp, duration, comp.size, to);
	}
	private static TweenSizeDelta Begin_impl (TweenSizeDelta comp, float duration, Vector2 from, Vector2 to) {
		comp.size = from;
		comp.from = from;
		comp.to = to;
		if (duration <= 0f) {
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}
}
