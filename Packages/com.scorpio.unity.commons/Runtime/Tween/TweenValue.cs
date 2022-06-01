using UnityEngine;
public class TweenValue : UITweener {
    private AnimationOrTween.OnSample sample;
    protected override void OnUpdate(float factor, bool isFinished) { if (sample != null) sample(factor, isFinished); }
    public static TweenValue Begin(UnityEngine.Object obj, float duration, AnimationOrTween.OnSample sample) {
        if (obj == null) { return null; }
        return Begin_impl(UITweener.Begin<TweenValue>(obj, duration), duration, sample);
    }
    private static TweenValue Begin_impl(TweenValue comp, float duration, AnimationOrTween.OnSample sample) {
        comp.sample = sample;
        if (duration <= 0f) {
            comp.Sample(1f, true);
            comp.enabled = false;
        }
        return comp;
    }
}
