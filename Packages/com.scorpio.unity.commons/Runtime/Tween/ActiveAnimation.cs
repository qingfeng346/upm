using UnityEngine;
using AnimationOrTween;
public class ActiveAnimation : MonoBehaviour {
	public OnFinished<ActiveAnimation> onFinished;		//播放完动作回调
    bool mNotify = false;		//通知
	Animation mAnimation;		//Animation
	AnimationState mState;		//Animation 信息
	float mTime = 0;			//

	public bool isPlaying {
		get {
			foreach (AnimationState state in mAnimation) {
				if (!mAnimation.IsPlaying(state.name)) continue;
				if (state.time < state.length) return true;
			}
			return false;
		}
	}
	public void Reset () {
		if (mAnimation != null) {
			foreach (AnimationState state in mAnimation) {
				state.time = 0f;
			}
			mAnimation.Sample();
		}
	}
	void Update () {
		if (mState != null) {
			float delta = Time.deltaTime;
			if (delta == 0f) return;
			mTime += mState.speed * delta;
			if (mTime < mState.length) {
				return;
			} else {
				mState.time = mState.length;
				mAnimation.Sample ();
			}
			enabled = false;
		} else {
			enabled = false;
			return;
		}
		if (mNotify) {
			mNotify = false;
			if (onFinished != null) {
				onFinished.Invoke (this);
            	onFinished = null;
			}
		}
	}
	void Play (string clipName) {
		enabled = true;
		mNotify = true;
		mAnimation.enabled = true;
		mAnimation.Stop ();
		mState = null;
		mTime = 0;
		if (string.IsNullOrEmpty (clipName)) {
			mAnimation.Play ();
		} else {
			mAnimation.Play (clipName);
		}
		foreach (AnimationState state in mAnimation) {
			if (mAnimation.IsPlaying (state.name)) {
				state.time = 0;
				mState = state;
				break;
			}
		}
		mAnimation.Sample ();
	}
	static public ActiveAnimation Play (UnityEngine.Object obj, string clipName) {
		return Play(obj, clipName, null);
	}
	static public ActiveAnimation Play (UnityEngine.Object obj, string clipName, AnimationOrTween.OnFinished<ActiveAnimation> onFinished) {
		return PlayAnimation(EngineUtil.GetComponent<Animation>(obj), clipName, onFinished);
	}
	//播放一个动画
	static public ActiveAnimation PlayAnimation (Animation animation, string clipName, AnimationOrTween.OnFinished<ActiveAnimation> onFinished) {
		if (animation == null) return null;
		var anim = EngineUtil.GetComponent<ActiveAnimation> (animation);
		anim.mAnimation = animation;
		anim.onFinished = onFinished;
		anim.Play(clipName);
		return anim;
	}
}
