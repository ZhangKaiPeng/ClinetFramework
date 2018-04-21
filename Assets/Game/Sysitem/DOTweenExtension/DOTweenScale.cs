using UnityEngine;

[AddComponentMenu("DOTweenUtils/Tween Scale")]
public class DOTweenScale : DOTweenUtil {

    public Vector3 From;
    public Vector3 To;

	public bool SetFrom2CurValue = false;

    public Vector3 value
    {
        get
        {
            return _target.localScale;
        }
    }

    protected override void SetupDOTweener()
    {   

		Vector3 tempScale = From;

		if(SetFrom2CurValue) 
		{
			From = value;
			tempScale = value;
		}
		
        _target.localScale = From;

        _DOTweener = DG.Tweening.DOTween.To(
            ()=>tempScale,
            (x)=>{

            _target.localScale = x;
            tempScale = x;

        },To,Duration);

        base.SetupDOTweener();
    }

    [ContextMenu("Set 'From' to Current Value")]
    public override void SetStartToCurrentValue()
    {
        base.SetStartToCurrentValue();

        From = _target.localScale;
    }

    [ContextMenu("Set 'To' to Current Value")]
    public override void SetEndToCurrentValue()
    {
        base.SetEndToCurrentValue();

        To = _target.localScale;
    }
}
