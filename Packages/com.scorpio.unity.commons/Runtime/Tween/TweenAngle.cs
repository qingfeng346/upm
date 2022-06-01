using UnityEngine;
public class TweenAngle : UITweener
{
	public Vector3 from;
	public Vector3 to;
	Transform mTrans;
	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

	override protected void OnUpdate (float factor, bool isFinished)
	{
		cachedTransform.localEulerAngles = from * (1f - factor) + to * factor;
	}
	static public TweenAngle Begin (UnityEngine.Object go, float duration, Vector3 angle)
	{
		TweenAngle comp = UITweener.Begin<TweenAngle>(go, duration);
		comp.from = comp.cachedTransform.localEulerAngles;
		comp.to = angle;
		return comp;
	}
}