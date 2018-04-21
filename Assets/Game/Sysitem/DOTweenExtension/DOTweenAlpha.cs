using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[AddComponentMenu("DOTweenUtils/Tween Alpha")]
public class DOTweenAlpha : DOTweenUtil {

    public float From = 1;
    public float To = 1;

    public float CurAlpha = 0;

    public bool IncludeChild = false;

	public bool ResetAlphaOnTweenReset = false;

    private CanvasGroup _canvasGroups;

    private Material _mat;
    private Material[] _matArray = null;

    private Light _light;
    private Light[] _lightArray = null;

    private SpriteRenderer _sprite;
    private SpriteRenderer[] _spriteArray = null;

    protected override void SetupDOTweener()
    {
        CacheAlphaComponent();

//        SetAlpha(From);

        float tempAlpha = From;

        _DOTweener = DG.Tweening.DOTween.To(
            ()=>tempAlpha,
            (x)=>{
            SetAlpha(x);
            tempAlpha = x;
        },To,Duration);

        base.SetupDOTweener();
    }

    private void CacheAlphaComponent()
    {
        _canvasGroups = _target.GetComponent<CanvasGroup>();
        if (null == _canvasGroups)
        {
            if (null != _target.GetComponent<CanvasRenderer>())
            {
                _canvasGroups = _target.gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (false == IncludeChild)
        {
            Renderer tempRender = _target.GetComponent<Renderer>();
            if (null != tempRender)
            {
                _mat = tempRender.material;
            }

            _light = _target.GetComponent<Light>();

            _sprite = _target.GetComponent<SpriteRenderer>();   
        }
        else
        {
            Renderer[] tempRenderArray = _target.GetComponentsInChildren<Renderer>(true);
            if (null != tempRenderArray && tempRenderArray.Length > 0)
            {
                _matArray = new Material[tempRenderArray.Length];
                for(int i = _matArray.Length - 1 ; i>=0 ; i--)
                {
                    _matArray[i] = tempRenderArray[i].material;
                }
            }

            _lightArray = _target.GetComponentsInChildren<Light>(true);
            _spriteArray = _target.GetComponentsInChildren<SpriteRenderer>(true);
        }
    }

    public void SetAlpha(float alpha)
    {        
        CurAlpha = alpha;
        if (null != _canvasGroups)
        {
            _canvasGroups.alpha = alpha;
        }
        Color tempColor;
        if (false == IncludeChild)
        {
            if (null != _mat)
            {
                tempColor = _mat.color;
                tempColor.a = alpha;
                _mat.color = tempColor;
            }
            if (null != _light)
            {
                tempColor = _light.color;
                tempColor.a = alpha;
                _light.color = tempColor;
            }
            if (null != _sprite)
            {
                tempColor = _sprite.color;
                tempColor.a = alpha;
                _sprite.color = tempColor;
            }
        }
        else
        {
            int i;
            if (null != _matArray && _matArray.Length > 0)
            {
                for (i = _matArray.Length - 1; i >= 0; i--)
                {
                    tempColor = _matArray[i].color;
                    tempColor.a = alpha;
                    _matArray[i].color = tempColor;
                }
            }
            if (null != _lightArray && _lightArray.Length > 0)
            {
                for (i = _lightArray.Length - 1; i >= 0; i--)
                {
                    tempColor = _lightArray[i].color;
                    tempColor.a = alpha;
                    _lightArray[i].color = tempColor;
                }
            }
            if (null != _spriteArray && _spriteArray.Length > 0)
            {
                for (i = _spriteArray.Length - 1; i >= 0; i--)
                {
                    tempColor = _spriteArray[i].color;
                    tempColor.a = alpha;
                    _spriteArray[i].color = tempColor;
                }
            }
        }

    } 

	public override void Play (bool forward)
	{
		if (null != _DOTweener && _DOTweener.IsPlaying())
		{
			base.Play(forward);
		}
		else
		{
			Restart();
			if(null != _DOTweener) _DOTweener.ChangeValues(forward?From:To,forward?To:From);
			PlayForward();
		}
	}

	public override void ResetTween ()
	{
		if(ResetAlphaOnTweenReset) SetAlpha(From);
		
		base.ResetTween ();
	}
}
