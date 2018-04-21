using DG.Tweening;
using UnityEngine;

public abstract class IDOTweenUtil : ImplMonoBehaviour
{
	public abstract Tween GetTween();

	public abstract void SetTarget(Transform target);

	public abstract void SetResetOnDisable(bool value);

	public abstract void SetAutoPlayOnEnable(bool value);

	public abstract void ResetTween();

	public abstract void Pause();

	public abstract void Play(bool forward);

	public abstract void AppendCompleteCallback(UnityEngine.Events.UnityAction action);
}
