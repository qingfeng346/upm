using System;
using UnityEngine;

public sealed class ActiveAnimator : MonoBehaviour {
    private static readonly int s_DefaultStateHash = Animator.StringToHash ("default");

    public Action<object, ActiveAnimator> onFinished; //播放完动作回调
    private bool mNotify = false; //通知
    private object mArgs = null;
    private Animator mAnimator;
    private AnimationClip mClip;

    public bool isPlaying => Mathf.Clamp01 (mAnimator.GetCurrentAnimatorStateInfo (0).normalizedTime) < 0.995f;

    private void Update () {
        var stateInfo = mAnimator.GetCurrentAnimatorStateInfo (0);
        if (Mathf.Clamp01 (stateInfo.normalizedTime) <= 0.995f)
            return;
        if (mAnimator.HasState (0, s_DefaultStateHash)) {
            mAnimator.CrossFade (s_DefaultStateHash, 0);
            mAnimator.Update (0);
            mAnimator.Update (0);
        }
        if (!mClip.isLooping) {
            mAnimator.enabled = false;
        }
        enabled = false;
        if (mNotify) {
            mNotify = false;
            if (onFinished != null) {
				onFinished.Invoke (mArgs, this);
            	onFinished = null;
			}
        }
    }

    private void Play (string clipName, float normalizedTime) {
        enabled = true;
        mNotify = true;
        mClip = Array.Find (mAnimator.runtimeAnimatorController.animationClips, (clip) => clip.name == clipName);
        if (mClip != null) {
            mAnimator.enabled = true;
            mAnimator.Play (clipName, 0, normalizedTime);
            mAnimator.Update (0);
        } else {
            enabled = false;
            mNotify = false;
            mAnimator.enabled = false;
        }
    }

    //播放一个动画
    public static ActiveAnimator Play (Animator animator, string clipName, float normalizedTime, Action<object, ActiveAnimator> onFinished, object args) {
        if (animator == null)
            return null;
        var anim = EngineUtil.GetComponent<ActiveAnimator> (animator);
        anim.mAnimator = animator;
        anim.onFinished = onFinished;
        anim.mArgs = args;
        anim.Play (clipName, normalizedTime);
        return anim;
    }
}