using UnityEngine;
using System.Collections;


namespace eeGames.Widget
{

    /// <summary>
    /// This Class is under development so it is subject to changes 
    /// </summary>
     
    [RequireComponent(typeof(CanvasGroup))]
    public class Widget : MonoBehaviour, IWidget
    {
        private WidgetName m_id;
        /// <summary>
        /// Widget Id (name)
        /// </summary>
        public WidgetName Id
        {
            get { return m_id; }
            set { m_id = value; }
        }

        private bool m_isActive;
        /// <summary>
        /// Tells if Widget is visible or not
        /// </summary>
        public bool IsActive
        {
            get { return m_isActive; }
            set { m_isActive = value; }
        }

        private bool m_isPool;
        /// <summary>
        /// Don't destroy Widget once it gets load.
        /// </summary>
        public bool IsPoolOnLoad
        {
            get { return m_isPool; }
            set { m_isPool = value; }
        }

        private Tweens m_tween;
        /// <summary>
        /// Tween Componet Contains Position | Scale | Rotation Tweens
        /// </summary>
        public Tweens Tweens
        {
            get
            {
                if (m_tween == null)
                {
                    var wTween = GetComponent<WidgetTween>();
                    if (wTween != null) m_tween = wTween.TweenData;
                }

                return m_tween;
            }
            set { m_tween = value; }
        }


        private CanvasGroup m_canvasGroup;
        /// <summary>
        /// canvas group used to disable previous widgets
        /// </summary>
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (m_canvasGroup == null)
                {
                    var cGroup = GetComponent<CanvasGroup>();
                    if (cGroup != null) m_canvasGroup = cGroup;
                    else Debug.Log("<color=red>Attach CanvasGroup with Widget GameObject:</color>");
                }

                return m_canvasGroup;
            }
        }

        /// <summary>
        /// OnShow Event Add Listner to this to be played on Widget Show
        /// </summary>
        public WidgetEvent OnShow;
        /// <summary>
        /// OnHide Event Add Listner to this to be played on Widget Hide
        /// </summary>
        public WidgetEvent OnHide;

        protected virtual void Awake()
        {
            var t = GetComponent<WidgetTween>();
            if (t != null) Tweens = t.TweenData;
        }
        /// <summary>
        /// Gets called before Show , only get called when Widget Pushed.
        /// </summary>
        public virtual void Init()
        {

        }
        /// <summary>
        /// Explicitly called this function and do update logic in it.
        /// </summary>
        public virtual void UpdateWidget()
        {

        }
        /// <summary>
        /// Destroy Widget(gameobject)
        /// </summary>
        public virtual void DestroyWidget()
        {
            Destroy(gameObject);
        }
        /// <summary>
        /// Gets called When Widget Pushed.
        /// </summary>
        /// <param name="IsPlayTween"> Play OnHide Tween of Current Widget </param>
        public void Show(bool IsPlayTween = true)
        {
            if (OnShow != null) OnShow.Invoke();
            gameObject.SetActive(true);
            if (IsPlayTween)
            {
                if (Tweens != null) Tweens.PerformOnShowTweens(gameObject);
            }

            IsActive = true;
            CanvasGroup.interactable = true;
        }
        /// <summary>
        /// Gets called when Widget poped
        /// </summary>
        /// <param name="IsPlayTween"> Play OnShow Tween of previous widget </param>
        public void Hide(bool IsPlayTween = true)
        {
            if (OnHide != null) OnHide.Invoke();
            if (IsPlayTween)
            {
                if (Tweens != null)
                    Tweens.PerformOnHideTweens(gameObject, () => { gameObject.SetActive(false);});
            }
            else
            {
                gameObject.SetActive(false);
            }

            IsActive = false;
        }
        


//#if UNITY_EDITOR
//        void OnDrawGizmos() 
//        {
//            if (Tweens == null || Tweens.IsPosition == false) return;

//            string[] dimension = UnityEditor.UnityStats.screenRes.Split('x');
//            int _width = System.Int32.Parse(dimension[0]);
//            int _height = System.Int32.Parse(dimension[1]);

//            var fromPos = Tweens.sPosition.From;
//            fromPos.x *= _width;
//            fromPos.y *= _height;
          

//            var toPos = Tweens.sPosition.To;
//            toPos.x *= _width;
//            toPos.y *= _height;

//            var hidePos = Tweens.ePosition;
//            hidePos.x *= _width;
//            hidePos.y *= _height;

            
//            Gizmos.DrawWireCube(fromPos, new Vector3(_width, _height));
            
//            Gizmos.DrawWireCube(toPos, new Vector3(_width, _height));
            
//            Gizmos.DrawWireCube(hidePos, new Vector3(_width, _height));
//        }
//#endif

    }
}