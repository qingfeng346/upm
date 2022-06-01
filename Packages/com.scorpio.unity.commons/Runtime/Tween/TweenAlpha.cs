using UnityEngine;
using UnityEngine.UI;
public class TweenAlpha : UITweener {
	public float from = 1f;
	public float to = 1f;
	SpriteRenderer mRenderer = null;
    MaskableGraphic mWidget = null;
	CanvasGroup mGroup = null;
	void Awake () {
		mWidget = GetComponent<MaskableGraphic>();
		mRenderer = GetComponent<SpriteRenderer> ();
		mGroup = GetComponent<CanvasGroup>();
	}
	public float alpha {
		get {
			if (mWidget != null)
				return mWidget.color.a;
			if (mRenderer != null)
				return mRenderer.color.a;
			if (mGroup != null)
				return mGroup.alpha;
			return 0;
		}
		set {
			if (mWidget != null) {
				var color = mWidget.color;
				color.a = value;
				mWidget.color = color;
			}
			if (mRenderer != null) {
				var color = mRenderer.color;
				color.a = value;
				mRenderer.color = color;
			}
			if (mGroup != null) {
				mGroup.alpha = value;
			}
		}
	}
	protected override void OnUpdate (float factor, bool isFinished) { alpha = Mathf.Lerp(from, to, factor); }
	public static TweenAlpha Begin (UnityEngine.Object go, float duration, float to) {
		var comp = UITweener.Begin<TweenAlpha>(go, duration);
		return Begin_impl (comp, duration, comp.alpha, to);
	}
	public static TweenAlpha Begin (UnityEngine.Object go, float duration, float from, float to) {
		return Begin_impl (UITweener.Begin<TweenAlpha> (go, duration), duration, from, to);
	}
	private static TweenAlpha Begin_impl (TweenAlpha comp, float duration, float from, float to) {
		comp.alpha = from;
		comp.from = from;
		comp.to = to;
		if (duration <= 0f) {
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}
}
