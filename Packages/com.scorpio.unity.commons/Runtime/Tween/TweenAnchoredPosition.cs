using UnityEngine;
public class TweenAnchoredPosition : UITweener {
	public Vector3 from;
	public Vector3 to;
	RectTransform mTrans;
	void Awake () {
		mTrans = GetComponent<RectTransform>();
	}
	public Vector3 position { 
		get {
			return mTrans != null ? mTrans.anchoredPosition3D : Vector3.zero;
		} 
		set {
			if (mTrans != null) mTrans.anchoredPosition3D = value;
		} 
	}
	protected override void OnUpdate (float factor, bool isFinished) { position = from * (1f - factor) + to * factor; }
    public static TweenAnchoredPosition Begin(UnityEngine.Object go, float duration, Vector2 from, Vector2 to) {
        return Begin(go, duration, new Vector3(from.x, from.y, 0), new Vector3(to.x, to.y, 0));
    }
    public static TweenAnchoredPosition Begin (UnityEngine.Object go, float duration, Vector3 from, Vector3 to) {
		return Begin_impl (UITweener.Begin<TweenAnchoredPosition> (go, duration), duration, from, to);
	}
    public static TweenAnchoredPosition Begin(UnityEngine.Object go, float duration, Vector2 to) {
        return Begin(go, duration, new Vector3(to.x, to.y, 0));
    }
    public static TweenAnchoredPosition Begin (UnityEngine.Object go, float duration, Vector3 to) {
		TweenAnchoredPosition comp = UITweener.Begin<TweenAnchoredPosition>(go, duration);
		return Begin_impl (comp, duration, comp.position, to);
	}
	private static TweenAnchoredPosition Begin_impl (TweenAnchoredPosition comp, float duration, Vector3 from, Vector3 to) {
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
