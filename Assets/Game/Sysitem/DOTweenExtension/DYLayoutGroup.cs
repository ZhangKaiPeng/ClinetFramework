using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

//[DisallowMultipleComponent]
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public abstract class DYLayoutGroup : UIBehaviour, ILayoutElement, ILayoutGroup
{
    [SerializeField] protected RectOffset m_Padding = new RectOffset();
    public RectOffset padding { get { return m_Padding; } set { SetProperty(ref m_Padding, value); } }

    [FormerlySerializedAs("m_Alignment")]
    [SerializeField] protected TextAnchor m_ChildAlignment = TextAnchor.UpperLeft;
    public TextAnchor childAlignment { get { return m_ChildAlignment; } set { SetProperty(ref m_ChildAlignment, value); } }

    [System.NonSerialized] private RectTransform m_Rect;
    protected RectTransform rectTransform
    {
        get
        {
            if (m_Rect == null)
                m_Rect = GetComponent<RectTransform>();
            return m_Rect;
        }
    }

    protected DrivenRectTransformTracker m_Tracker;
    private Vector2 m_TotalMinSize = Vector2.zero;
    private Vector2 m_TotalPreferredSize = Vector2.zero;
    private Vector2 m_TotalFlexibleSize = Vector2.zero;

    [System.NonSerialized] private List<RectTransform> m_RectChildren = new List<RectTransform>();
    protected List<RectTransform> rectChildren { get { return m_RectChildren; } }

    [System.NonSerialized]
    private Dictionary<RectTransform,DOTweenSpring> m_RectChildrenTweener = new Dictionary<RectTransform, DOTweenSpring>();

    private DOTweenSpring m_SpringController; 
    public DOTweenSpring SpringController
    {
        get
        {
            m_SpringController = this.GetComponent<DOTweenSpring>();
            if (null == m_SpringController)
            {
                m_SpringController = this.gameObject.AddComponent<DOTweenSpring>();

                m_SpringController.LoopTime = 1;
                m_SpringController.TweenGroup = "DYLayoutGroup";
                m_SpringController.Duration = 0.5f;
                m_SpringController.Init(this,true);

                m_SpringController.OnDOTweenComplete.AddListener(OnTweenCompleteCallback);
            }
            return m_SpringController;
        }
    }

    public bool RepositionNow
    {
        get{ return m_repositionNow;}   
        set
        {            
            m_repositionNow = value;
            SetDirty();
        }
    }
    private bool m_repositionNow = false;
    private bool m_NeedReposition = false;

    protected DOTweenSpring GetRectChildrenTweener(RectTransform rect)
    {        
        DOTweenSpring tweener;
        if (m_RectChildrenTweener.TryGetValue(rect, out tweener) == false)
        {
            tweener = DOTweenSpring.CreateInstance(rect);
            if (tweener.IsController == false)
            {
                tweener.Init(this,false);
                m_RectChildrenTweener.Add(rect,tweener);//cache one
            }
        }

        return tweener;
    }

    // ILayoutElement Interface
    public virtual void CalculateLayoutInputHorizontal()
    {        
        
        m_RectChildren.Clear();     

        for (int i = 0; i < rectTransform.childCount; i++)
        {
            RectTransform rect = rectTransform.GetChild(i) as RectTransform;
            if (rect == null)
                continue;
            ILayoutIgnorer ignorer = rect.GetComponent(typeof(ILayoutIgnorer)) as ILayoutIgnorer;
            if (rect.gameObject.activeInHierarchy && !(ignorer != null && ignorer.ignoreLayout))
            {
                m_RectChildren.Add(rect);
            }
        }

        m_Tracker.Clear();
    }

    public abstract void CalculateLayoutInputVertical();
    public virtual float minWidth { get { return GetTotalMinSize(0); } }
    public virtual float preferredWidth { get { return GetTotalPreferredSize(0); } }
    public virtual float flexibleWidth { get { return GetTotalFlexibleSize(0); } }
    public virtual float minHeight { get { return GetTotalMinSize(1); } }
    public virtual float preferredHeight { get { return GetTotalPreferredSize(1); } }
    public virtual float flexibleHeight { get { return GetTotalFlexibleSize(1); } }
    public virtual int layoutPriority { get { return 0; } }

    // ILayoutController Interface

    public abstract void SetLayoutHorizontal();
    public abstract void SetLayoutVertical();

    // Implementation

    protected DYLayoutGroup()
    {
        if (m_Padding == null)
            m_Padding = new RectOffset();
    }

    #region Unity Lifetime calls

    protected override void Start()
    {
        base.Start();

        SpringController.OnDOTweenComplete.RemoveListener(OnTweenCompleteCallback);//in case duplicate callBack func
        SpringController.OnDOTweenComplete.AddListener(OnTweenCompleteCallback);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetDirty();

        RepositionNow = false;
    }

    protected override void OnDisable()
    {
        m_Tracker.Clear();
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        base.OnDisable();
    }

    protected override void OnDidApplyAnimationProperties()
    {
        SetDirty();
    }

    #endregion

    protected float GetTotalMinSize(int axis)
    {
        return m_TotalMinSize[axis];
    }

    protected float GetTotalPreferredSize(int axis)
    {
        return m_TotalPreferredSize[axis];
    }

    protected float GetTotalFlexibleSize(int axis)
    {
        return m_TotalFlexibleSize[axis];
    }

    protected float GetStartOffset(int axis, float requiredSpaceWithoutPadding)
    {
        float requiredSpace = requiredSpaceWithoutPadding + (axis == 0 ? padding.horizontal : padding.vertical);
        float availableSpace = rectTransform.rect.size[axis];
        float surplusSpace = availableSpace - requiredSpace;
        float alignmentOnAxis = 0;
        if (axis == 0)
            alignmentOnAxis = ((int)childAlignment % 3) * 0.5f;
        else
            alignmentOnAxis = ((int)childAlignment / 3) * 0.5f;
        return (axis == 0 ? padding.left : padding.top) + surplusSpace * alignmentOnAxis;
    }

    protected void SetLayoutInputForAxis(float totalMin, float totalPreferred, float totalFlexible, int axis)
    {
        m_TotalMinSize[axis] = totalMin;
        m_TotalPreferredSize[axis] = totalPreferred;
        m_TotalFlexibleSize[axis] = totalFlexible;
    }

    protected void SetChildAlongAxis(RectTransform rect, int axis, float pos, float size)
    {
        if (rect == null)
            return;

        m_Tracker.Add(this, rect,
            DrivenTransformProperties.Anchors |
            DrivenTransformProperties.AnchoredPosition |
            DrivenTransformProperties.SizeDelta);

        rect.SetInsetAndSizeFromParentEdge(axis == 0 ? RectTransform.Edge.Left : RectTransform.Edge.Top, pos, size);
    }

    protected void SetChildAlongAxis(RectTransform rect, int axis, float pos, float size,bool isTweenMove,bool isAutoMove,bool ignoreScale)
    {
        if (false == isTweenMove)
        {
            SetChildAlongAxis(rect, axis, pos, size);
            return;
        }

        Vector4 tempPosNSize = new Vector4(
            rect.anchoredPosition.x - rect.sizeDelta.x * rect.pivot.x,
            rect.sizeDelta.x,
            -(rect.anchoredPosition.y + rect.sizeDelta.y * rect.pivot.y),
            rect.sizeDelta.y);

        if (ignoreScale)
        {            
            if((axis==1?tempPosNSize.w:tempPosNSize.y) != size)
            {
                SetChildAlongAxis(rect,axis,(axis==1?tempPosNSize.z:tempPosNSize.x),size);
            }
        }

        //pos and size did not change(当前位置与目标位置的比较)
        if ( (axis == 1 && tempPosNSize.z == pos && tempPosNSize.w == size)
            || (axis == 0 && tempPosNSize.x == pos && tempPosNSize.y == size))
        {            
            return;
        }

        DOTweenSpring tempTweenSpring = GetRectChildrenTweener(rect);

        Vector4 targetPosNSize = axis == 1 ? new Vector4(tempTweenSpring.EndValule.x,tempTweenSpring.EndValule.y,pos,size) : new Vector4(pos,size,tempTweenSpring.EndValule.z,tempTweenSpring.EndValule.w);

        //当前位置与Tween的目标位置比较(若当前正在缓动到正确的位置,则略过下面多余的判断)
        if ((targetPosNSize - tempPosNSize).sqrMagnitude < 0.01f)
        {
            return;
        }
            
        //说明当前layout需要更新!(尚未完成更新或者未开始更新)
        if(false == m_NeedReposition) m_NeedReposition = true;

        if (isAutoMove == true)
        {
            ResetPosition();
        }

        if (false == RepositionNow)
        {
            return;
        }

        if (tempTweenSpring.IsPlaying())
        {
            if (tempTweenSpring.EndValule != targetPosNSize)//pos change while tweening
            {
                tempTweenSpring.Kill();
            }
        }

        if (tempTweenSpring.IsKilled())
        {
            //DYDYLogger.LogFormat("SetChildAlongAxis object={0} targetPosNSize={1} originPosNSize={2}", rect.name, targetPosNSize, tempPosNSize);                            

            tempTweenSpring.BeginTween(
                
                tempPosNSize,
                targetPosNSize,
                (x)=>{

                tempPosNSize = x;
                SetChildAlongAxis(rect, 1, tempPosNSize.z, tempPosNSize.w);
                SetChildAlongAxis(rect, 0, tempPosNSize.x, tempPosNSize.y);
            });
        }
    }

    private bool isRootLayoutGroup
    {
        get
        {
            Transform parent = transform.parent;
            if (parent == null)
                return true;
            return transform.parent.GetComponent(typeof(ILayoutGroup)) == null;
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        if (isRootLayoutGroup)
            SetDirty();
    }

    protected virtual void OnTransformChildrenChanged()
    {
        SetDirty();
    }

    protected void SetProperty<T>(ref T currentValue, T newValue)
    {
        if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
            return;
        currentValue = newValue;
        SetDirty();
    }

    protected void SetDirty()
    {
        if (!IsActive())
            return;

        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    protected virtual void OnTweenCompleteCallback()
    {
        RepositionNow = false;
        m_NeedReposition = false;
    }

    #region public func
    public virtual void ResetPosition()
    {        
        if (false == m_NeedReposition)
            return;

        if (false == RepositionNow)
        {
            if(null != m_SpringController)
                m_SpringController.DOTweenStart();
            
            RepositionNow = true;
        }
        SetDirty();
    }
    #endregion

    #if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetDirty();
    }

    #endif
}