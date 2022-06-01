using UnityEngine;
using UnityEngine.UI;
public class TweenColor : UITweener {
	public Color from = Color.white;
	public Color to = Color.white;
	SpriteRenderer mRenderer;
    MaskableGraphic mWidget;
	void Awake () {
		mWidget = GetComponent<MaskableGraphic> ();
		mRenderer = GetComponent<SpriteRenderer> ();
	}
	public Color color {
		get {
			if (mWidget != null)
				return mWidget.color;
			if (mRenderer != null)
				return mRenderer.color;
			return Color.white;
		}
		set {
			if (mWidget != null) 
				mWidget.color = value;
			if (mRenderer != null)
				mRenderer.color = value;
		}
	}
	protected override void OnUpdate(float factor, bool isFinished) { color = Color.Lerp(from, to, factor); }
	public static TweenColor Begin (UnityEngine.Object go, float duration, Color from, Color to) {
		return Begin_impl (UITweener.Begin<TweenColor> (go, duration), duration, from, to);
	}
	public static TweenColor Begin (UnityEngine.Object go, float duration, Color to) {
		TweenColor comp = UITweener.Begin<TweenColor>(go, duration);
		return Begin_impl (comp, duration, comp.color, to);
	}
	private static TweenColor Begin_impl (TweenColor comp, float duration, Color from, Color to) {
		comp.color = from;
		comp.from = from;
		comp.to = to;
		if (duration <= 0f) {
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}
}
