using UnityEngine;

[AddComponentMenu("DOTweenUtils/Tween Rotation")]
public class DOTweenRotation : DOTweenUtil {

    public Vector3 From;
    public Vector3 To;

	public bool SetFrom2CurValue = false;
	public bool EnsureFinalValue = false;

	public bool WorldSpace = true;

	public bool IgnoreX = false;
	public bool IgnoreY = false;
	public bool IgnoreZ = false;

    public Quaternion value
    {
        get
        {
			return WorldSpace ? _target.rotation : _target.localRotation;
        }
		set
		{
			if(WorldSpace) _target.rotation = value;
			else _target.localRotation = value;
		}
    }

    protected override void SetupDOTweener()
    {
//        _target.rotation = Quaternion.Euler(From);

		Quaternion tempRotate;
		if(SetFrom2CurValue) 
		{
			From = value.eulerAngles;
			tempRotate = value;
		}
		else tempRotate = Quaternion.Euler(From);
		//        Vector3 tempRotate = From;

        _DOTweener = DG.Tweening.DOTween.To(
            ()=>tempRotate,
            (x)=>{
				if(IgnoreX || IgnoreY || IgnoreZ)
				{
					Vector3 tempVec = tempRotate.eulerAngles;
					Vector3 curVec = value.eulerAngles;
					if(IgnoreX) tempVec.x = curVec.x;
					if(IgnoreY) tempVec.y = curVec.y;
					if(IgnoreZ) tempVec.z = curVec.z;
					value = Quaternion.Euler(tempVec);					
				}else value = tempRotate;

            	tempRotate = x;

        }, To ,Duration);

        base.SetupDOTweener();
    }
    
	protected override void OnDOTweenCompleteCallback ()
	{
		if(EnsureFinalValue) 
		{
			Vector3 finialRotat = _DOTweener.isBackwards ? From : To;

			if(IgnoreX || IgnoreY || IgnoreZ)
			{
				Vector3 tempVec = finialRotat;
				Vector3 curVec = value.eulerAngles;
				if(IgnoreX) tempVec.x = curVec.x;
				if(IgnoreY) tempVec.y = curVec.y;
				if(IgnoreZ) tempVec.z = curVec.z;

				finialRotat = tempVec;
			}

			value = Quaternion.Euler(finialRotat);
		}
		
		base.OnDOTweenCompleteCallback ();
	}

    [ContextMenu("Set 'From' to Current Value")]
    public override void SetStartToCurrentValue()
    {
        base.SetStartToCurrentValue();

		From = value.eulerAngles;
    }
        
    [ContextMenu("Set 'To' to Current Value")]
    public override void SetEndToCurrentValue()
    {
        base.SetEndToCurrentValue();

		To = value.eulerAngles;
    }
}
