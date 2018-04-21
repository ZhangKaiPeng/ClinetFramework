using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class DOTweenSpring : DOTweenUtil
{
    public Vector4 EndValule;

    public Vector4 StartValue;

    /// <summary>
    /// 该组件所属的LayoutGroup(同时作为Tween的GroupID)
    /// </summary>
    public DYLayoutGroup AttachedLayoutGroup{get{ return _layoutGroup;}}
    private DYLayoutGroup _layoutGroup;
    public DOTweenSpring SpringController{get { return _springController;}}
    private DOTweenSpring _springController;

    public bool IsController
    {
        get
        {      
            #if UNITY_EDITOR
            if(Application.isPlaying == false)
            {
                return null != this.GetComponent<DYLayoutGroup>();
            }
            #endif
            return _isController && null != _springController;
        }   
    }
    private bool _isController = false;

//    private int _springPlayCount = 0;
//    private int _springPauseCount = 0;
    private int _springCompleteCount = 0;
//    private int _springStartCount = 0;

    private DOTweenSpring(){}

    public static DOTweenSpring CreateInstance(RectTransform rect)
    {
        DOTweenSpring tempTweenerSpring = rect.GetComponent<DOTweenSpring>();
        if (null == tempTweenerSpring)
        {
            tempTweenerSpring = rect.gameObject.AddComponent<DOTweenSpring>();
        }
        return tempTweenerSpring;
    }

    public void Init(DYLayoutGroup layoutGroup,bool isController)
    {
        if(_layoutGroup != layoutGroup) _layoutGroup = layoutGroup;

        if (_isController != isController)
        {            
            _isController = isController || null != this.GetComponent<DYLayoutGroup>();
        }

        if (null == _layoutGroup)
            return;

        _springController = _layoutGroup.SpringController;

        if (this.transform.parent != _layoutGroup.transform && this.transform != _layoutGroup.transform)
        {
            Debug.LogWarningFormat("当前GameObject({0})不处于其管理的LayoutGrouop({1})下?!这将可能引起异常!",this.name,_layoutGroup.transform.name);
        }
    }

    protected override void Reset()
    {
        base.Reset();

        #if UNITY_EDITOR
        DYLayoutGroup layoutGroup = this.GetComponent<DYLayoutGroup>();

        if (null != layoutGroup)
        {
            DOTweenSpring[] tempList = this.GetComponents<DOTweenSpring>();
            if (null != tempList)
            {
                if (tempList.Length > 1)
                {
                    UnityEditor.EditorUtility.DisplayDialog("Error","Can not add two DOTweenSpring in one LayoutGroup","I see");

                    GameObject.Destroy(this);
                }
            }
        }
        #endif
    }

    protected override void Start()
    {
        //base.Start();
        Target = this.transform;
        DYLayoutGroup layoutGroup = this.GetComponent<DYLayoutGroup>();

        if (null != layoutGroup)
        {
            Init(layoutGroup,true);
        }
    }

    protected override void SetupDOTweener()
    {
        //base.SetupDOTweener();
    }

    protected override void OnDOTweenCompleteCallback()
    {
        if (true == IsController)
        {
            if(_springCompleteCount > 0) _springCompleteCount--;

            if (_springCompleteCount > 0)
            {
                return;
            }
        }
        else
        {
            if(_springController.IsController)//in case endless loop
            {
                _springController.OnDOTweenCompleteCallback();
            }
        }

        base.OnDOTweenCompleteCallback();

        //Kill();

    }

    protected override void OnDOTweenStartCallback()
    {
        base.OnDOTweenStartCallback();
    }

    protected override void OnDOTweenPauseCallback()
    {
        if (false == IsController)
        {
            Kill();
        }
    }
                
    public override void Play()
    {
        DOTween.Play(AttachedLayoutGroup);
    }        

    public override void Pause()
    {
        DOTween.Pause(AttachedLayoutGroup);
    }
       
    public override void PlayForward()
    {
        DOTween.PlayForward(AttachedLayoutGroup);
    }

    public override void PlayBackward()
    {
        DOTween.PlayBackwards(AttachedLayoutGroup);
    }

    public void BeginTween(Vector4 startValue,Vector4 endValue,DG.Tweening.Core.DOSetter<Vector4> setter)
    {
        EndValule = endValue;
        StartValue = startValue;

        if (false == IsController)//同步Controller的设置
        {            
            LoopTime = _springController.LoopTime;
            PlayStyle = _springController.PlayStyle;
            AnimationCurves = _springController.AnimationCurves;
            EaseType = _springController.EaseType;
            Duration = _springController.Duration;
            StartDelay = _springController.StartDelay;

            TweenGroup = _springController.TweenGroup;

            IgnoreTimeScale = _springController.IgnoreTimeScale;

//            OnDOTweenStart = _springController.OnDOTweenStart;
//            OnDOTweenComplete = _springController.OnDOTweenComplete;
//            OnDOTweenPlay = _springController.OnDOTweenPlay;
//            OnDOTweenPause = _springController.OnDOTweenPause;

            _springController.AddSpringEventCount();
        }

        Vector4 tempPosNSize = startValue;

        _DOTweener = DOTween.To(
            ()=>tempPosNSize,
            setter,
            endValue,Duration);
        
        base.SetupDOTweener();

        if (null != _layoutGroup)
        {
            _DOTweener.SetId(_layoutGroup);
        }

        if (this.enabled == false)
            this.enabled = true;
    }

    public void Kill()
    {        
        if (null != _springController)
        {
            _springController.RemoveSpringEventCount();
        }
        
        if (null != _DOTweener)
            _DOTweener.Kill();
        
        this.enabled = false;
    }

    public new bool IsPlaying()
    {
        return IsKilled() == false && _DOTweener.IsPlaying();
    }

    public bool IsKilled()
    {                
        return null == _DOTweener || _DOTweener.IsActive() == false || this.enabled == false;
    }
 
    public void AddSpringEventCount()
    {               
        //_springStartCount++;
        _springCompleteCount++;
    }

    public void RemoveSpringEventCount()
    {
        //if(_springStartCount > 0) _springStartCount--;
        if(_springCompleteCount > 0) _springCompleteCount--;
    }

    public void DOTweenStart()
    {
        OnDOTweenStartCallback();
    }
}