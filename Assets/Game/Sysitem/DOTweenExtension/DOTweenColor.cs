using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("DOTweenUtils/Tween Color")]
public class DOTweenColor : DOTweenUtil {

    public Color From = Color.white;
    public Color To = Color.white;
	
    private Image _image;
    private Text _text;
    private Material _mat;
    private Light _light;
    private SpriteRenderer _sprite;

    protected override void SetupDOTweener()
    {

        CacheColorComponent();

        SetColor(From);

        Color tempColor = From;

        _DOTweener = DG.Tweening.DOTween.To(
            ()=>tempColor,
            (x)=>{

            SetColor(x);
            tempColor = x;

        },To,Duration);

        base.SetupDOTweener();
    }

    private void CacheColorComponent()
    {
        _image = _target.GetComponent<Image>();
        _text = _target.GetComponent<Text>();
        Renderer tempRender = _target.GetComponent<Renderer>();
        if (null != tempRender)
        {
            _mat = tempRender.material;
        }
        _light = _target.GetComponent<Light>();
        _sprite = _target.GetComponent<SpriteRenderer>();
    }

    private void SetColor(Color color)
    {
        if (null != _image)
            _image.color = color;
        if (null != _text)
            _text.color = color;
        if (null != _mat)
            _mat.color = color;
        if (null != _light)
            _light.color = color;
        if (null != _sprite)
            _sprite.color = color;
    }         
}
