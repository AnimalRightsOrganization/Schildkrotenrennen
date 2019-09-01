//=================================================================================
// Tweening Utility for UI elements
// This class is under development, so it is subject to many changes 
//=================================================================================

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace eeGames.Actor
{
    public enum LoopType
    {
        PingPong,
        StartOver
    }
    public enum ActingType
    {
        Scale,
        Rotation,
        Position,
        Color
    }

    /// <summary>
    /// data structure used to store tween data
    /// </summary>
    [System.Serializable]
    public class ActorData
    {
        public bool IsActive;
        public bool IsActing;
        public float Time;
        public float DelayTime;
        public int TweenCount;
        public bool IsAutoPlay;
        public bool IsLoop;
        public bool IsOnce;
        public LeanTweenType TweenType;
        public LoopType LoopType;
        public Vector4 From;
        public Vector4 To;
        //        public Vector4 Hide;
    }

    [System.Serializable]
    public class ActorEvent : UnityEngine.Events.UnityEvent
    { }

    [System.Serializable]
    public class Actor : MonoBehaviour 
    {
        public ActingType Type;
        public ActorData ActorData = new ActorData();
        public ActorEvent OnStart;
        public ActorEvent OnStop;

        private bool _onFirstRun = true;
        void OnDisable() 
        {
            LeanTween.cancel(gameObject); 
        }
        void OnEnable() 
        {
            if (!_onFirstRun) { if (ActorData.IsAutoPlay) PerformActing(); }
        }

        void Start() 
        {
            if (ActorData.IsAutoPlay) PerformActing();
        }
       
        /// <summary>
        /// plays tween depending on which ActingType is selected 
        /// </summary>
         [ContextMenu("Perform Acting")]
        public void PerformActing()
        {
            if (!ActorData.IsActive) return;
            switch (Type)
            {
                case ActingType.Scale:
                    DoScaleActing();
                    break;
                case ActingType.Rotation:
                    DoRotationActing();
                    break;
                case ActingType.Position:
                    DoPositionActing();
                    break;
                case ActingType.Color:
                    DoColorActing();
                    break;
            }
            _onFirstRun = false;
        }

         [ContextMenu("Perform Reverse Acting")]
         public void PerformReverseActing()
         {
             if (!ActorData.IsActive) return;
           
             switch (Type)
             {

                 case ActingType.Scale:
                     DoReverseScaleActing();
                     break;
                 case ActingType.Rotation:
                     DoReverseRotationActing();
                     break;
                 case ActingType.Position:
                     DoReversePositionActing();
                     break;
                 case ActingType.Color:
                     DoReverseColorActing();
                     break;
             }
         }
         /// <summary>
         /// Setting tween from code at run time, useful if you wants to move object to other points
         /// </summary>
         /// <param name="From"></param>
         /// <param name="To"></param>
         /// <param name="Time"></param>
         /// <param name="Delay"></param>
         /// <param name="Type"></param>
         public void DoPositionTween(Vector3 From, Vector3 To, float Time, float Delay = 0f, LeanTweenType Type = LeanTweenType.linear)
         {
             gameObject.transform.position = From;
             LeanTween.move(gameObject, To, Time).setDelay(Delay).setLoopOnce().setEase(Type).setOnComplete(() => { if (OnStop != null) OnStop.Invoke(); });
         }


        #region Helper Acting Methods
       
        private void DoScaleActing() 
        {
            if (OnStart != null) OnStart.Invoke();
            var mainWindow = GetComponent<RectTransform>();

            if (!ActorData.IsOnce)
            {
                LeanTween.scale(mainWindow, ActorData.To, ActorData.Time).setDelay(ActorData.DelayTime).setLoopPingPong(ActorData.IsLoop ? -1 : ActorData.TweenCount).setEase(ActorData.TweenType).setOnComplete(() => { if (OnStop != null) OnStop.Invoke(); ActorData.IsActing = false; }).setOnStart(() => { mainWindow.transform.localScale = ActorData.From; ActorData.IsActing = true; });
            }
            else
            {

                LeanTween.scale(mainWindow, ActorData.To, ActorData.Time).setDelay(ActorData.DelayTime).setLoopOnce().setEase(ActorData.TweenType).setOnComplete(() => { if (OnStop != null) OnStop.Invoke(); }).setOnStart(() => { mainWindow.transform.localScale = ActorData.From; });
            }
        }

        private void DoReverseScaleActing()
        {
           
            var mainWindow = GetComponent<RectTransform>();

            if (!ActorData.IsOnce)
            {
                LeanTween.scale(mainWindow, ActorData.From, ActorData.Time).setDelay(ActorData.DelayTime).setLoopPingPong(ActorData.IsLoop ? -1 : ActorData.TweenCount).setEase(ActorData.TweenType).setOnComplete(() => { ActorData.IsActing = false; }).setOnStart(() => { mainWindow.transform.localScale = ActorData.To; ActorData.IsActing = true; });
            }
            else
            {

                LeanTween.scale(mainWindow, ActorData.From, ActorData.Time).setDelay(ActorData.DelayTime).setLoopOnce().setEase(ActorData.TweenType).setOnComplete(() => { if (OnStop != null) OnStop.Invoke(); }).setOnStart(() => { mainWindow.transform.localScale = ActorData.To; });
            }
        }

        private Vector3 _pos;
        private void DoReversePositionActing()
        {
            var mainWindow = GetComponent<RectTransform>();


#if UNITY_EDITOR
            string[] dimension = UnityEditor.UnityStats.screenRes.Split('x');
            int _width = System.Int32.Parse(dimension[0]);
            int _height = System.Int32.Parse(dimension[1]);
#endif

#if !UNITY_EDITOR
        int _width = Screen.width;
        int _height = Screen.height;
#endif

            var pos = ActorData.To;
            pos.x *= _width;
            pos.y *= _height;

   
            if (!ActorData.IsOnce)
            {
                LeanTween.move(mainWindow.gameObject, (Vector3)pos, ActorData.Time).setDelay(ActorData.DelayTime).setLoopPingPong(ActorData.IsLoop ? -1 : ActorData.TweenCount).setEase(ActorData.TweenType).setOnComplete(() => { ActorData.IsActing = false; }).setOnStart(() => { ActorData.IsActing = true; });
            }
            else
            {

                LeanTween.move(mainWindow.gameObject, _pos, ActorData.Time).setDelay(ActorData.DelayTime).setLoopOnce().setEase(ActorData.TweenType).setOnComplete(() => { });
            }
        }

       
        private void DoPositionActing()
        {

            if (OnStart != null) OnStart.Invoke();
            var mainWindow = GetComponent<RectTransform>();
       
#if UNITY_EDITOR
            string[] dimension = UnityEditor.UnityStats.screenRes.Split('x');
            int _width = System.Int32.Parse(dimension[0]);
            int _height = System.Int32.Parse(dimension[1]);
#endif

#if !UNITY_EDITOR
        int _width = Screen.width;
        int _height = Screen.height;
#endif

            var newPos = ActorData.From;
            newPos.x *= _width;
            newPos.y *= _height;
            mainWindow.position = newPos;
            _pos = newPos;
            var pos = ActorData.To;
            pos.x *= _width;
            pos.y *= _height;

            if (!ActorData.IsOnce) // TODO : actor moved to From position imediately (it should move to from position OnTweenStart)
            {
                if (ActorData.LoopType == LoopType.PingPong)
                {
                    LeanTween.move(mainWindow.gameObject, (Vector3)pos, ActorData.Time).setDelay(ActorData.DelayTime).setLoopPingPong(ActorData.IsLoop ? -1 : ActorData.TweenCount).setEase(ActorData.TweenType).setOnComplete(() => { if (OnStop != null) OnStop.Invoke(); ActorData.IsActing = false; }).setOnStart(() => { ActorData.IsActing = true; });
                }
                else
                {
                    LeanTween.move(mainWindow.gameObject, (Vector3)pos, ActorData.Time).setDelay(ActorData.DelayTime).setLoopOnce().setEase(ActorData.TweenType).setOnComplete(() => 
                    {
                        DoPositionActing();

                    });
                }     
            }
            else
            {
               
                LeanTween.move(mainWindow.gameObject, (Vector3)pos, ActorData.Time).setDelay(ActorData.DelayTime).setLoopOnce().setEase(ActorData.TweenType).setOnComplete(() => { if (OnStop != null) OnStop.Invoke(); });
            }

        }


        private void DoRotationActing()
        {
            
            if (OnStart != null) OnStart.Invoke();
            var mainWindow = GetComponent<RectTransform>();
          
            if (ActorData.IsLoop )
            {
     //           Debug.Log("beep beep boop");
                if ( ActorData.LoopType == LoopType.StartOver)
                {
                    // holy moly 
   //                 Debug.Log("holy moly");
                    LeanTween.rotateAroundLocal(mainWindow.gameObject, Vector3.forward, -360f * ActorData.From.z, ActorData.Time).setDelay(ActorData.DelayTime).setRepeat(-1).setEase(ActorData.TweenType).setOnComplete(() => { if (OnStop != null) OnStop.Invoke(); ActorData.IsActing = false; }).setOnStart(() => { mainWindow.transform.rotation = Quaternion.Euler(ActorData.From); ActorData.IsActing = true; });
                }
                else
                {
 //                   Debug.Log("crap");
                   LeanTween.rotate(mainWindow.gameObject, ActorData.To, ActorData.Time).setDelay(ActorData.DelayTime).setLoopPingPong(ActorData.IsLoop ? -1 : ActorData.TweenCount).setEase(ActorData.TweenType).setOnComplete(() => { if (OnStop != null) OnStop.Invoke(); ActorData.IsActing = false; }).setOnStart(() => { mainWindow.transform.rotation = Quaternion.Euler(ActorData.From); ActorData.IsActing = true; });
                }
                
            }
            if (ActorData.IsOnce && !ActorData.IsLoop)
            {

                LeanTween.rotate(mainWindow.gameObject, ActorData.To, ActorData.Time).setDelay(ActorData.DelayTime).setLoopClamp(ActorData.TweenCount).setEase(ActorData.TweenType).setOnComplete(() => { if (OnStop != null) OnStop.Invoke(); }).setOnStart(() => { mainWindow.transform.rotation = Quaternion.Euler(ActorData.From); });
            }
        }

        private void DoReverseRotationActing()
        {
            var mainWindow = GetComponent<RectTransform>();

            if (ActorData.IsOnce && !ActorData.IsLoop)
            {

                LeanTween.rotate(mainWindow.gameObject, ActorData.From, ActorData.Time).setDelay(ActorData.DelayTime).setLoopClamp(ActorData.TweenCount).setEase(ActorData.TweenType).setOnComplete(() => { if (OnStop != null) OnStop.Invoke(); }).setOnStart(() => { mainWindow.transform.rotation = Quaternion.Euler(ActorData.To); });
            }
        }


        private void DoColorActing()
        {
            
            if (OnStart != null) OnStart.Invoke();
            var mainWindow = GetComponent<RectTransform>();
            
            
            if(ActorData.IsLoop)
            {
                var uiImg = GetComponent<Image>();
                if (uiImg != null)
                {
                    if (ActorData.To.x != ActorData.From.x || ActorData.To.y != ActorData.From.y || ActorData.To.z != ActorData.From.z)
                    {
                        uiImg.color = ActorData.From;
                        LeanTween.color(mainWindow, ActorData.To, ActorData.Time).setDelay(ActorData.DelayTime).setLoopPingPong(ActorData.IsLoop ? -1 : ActorData.TweenCount).setEase(ActorData.TweenType).setOnComplete(() => { if (OnStop != null) OnStop.Invoke(); ActorData.IsActing = false; });
                    }

                }

                var canvasGroup = GetComponent<CanvasGroup>();
                canvasGroup.alpha = ActorData.From.w;
                if (ActorData.To.w != ActorData.From.w)
                {
                    LeanTween.alphaCanvas(canvasGroup, ActorData.To.w, ActorData.Time).setDelay(ActorData.DelayTime).setLoopPingPong(ActorData.IsLoop ? -1 : ActorData.TweenCount).setEase(ActorData.TweenType).setOnComplete(() => { if (OnStop != null) OnStop.Invoke(); ActorData.IsActing = false; });
                }
            }

            if (ActorData.IsOnce && !ActorData.IsLoop)
            {
                // here do loop once logic {don't care about copy paste above logic (happy lazy coding :))}
                var uiImg = GetComponent<Image>();
                if (uiImg != null)
                {
                    if (ActorData.To.x != ActorData.From.x || ActorData.To.y != ActorData.From.y || ActorData.To.z != ActorData.From.z)
                    {
                        uiImg.color = ActorData.From;
                        LeanTween.color(mainWindow, ActorData.To, ActorData.Time).setDelay(ActorData.DelayTime).setLoopOnce().setEase(ActorData.TweenType).setOnComplete(() => { if (OnStop != null) OnStop.Invoke(); ActorData.IsActing = false; });
                    }

                }

                var canvasGroup = GetComponent<CanvasGroup>();
                canvasGroup.alpha = ActorData.From.w;
                if (ActorData.To.w != ActorData.From.w)
                {
                    LeanTween.alphaCanvas(canvasGroup, ActorData.To.w, ActorData.Time).setDelay(ActorData.DelayTime).setLoopOnce().setEase(ActorData.TweenType).setOnComplete(() => { if (OnStop != null) OnStop.Invoke(); ActorData.IsActing = false; });
                }
            }
        }

        private void DoReverseColorActing()
        {
            var mainWindow = GetComponent<RectTransform>();
           
            if (ActorData.IsOnce && !ActorData.IsLoop)
            {
                // here do loop once logic {don't care about copy paste above logic (happy lazy coding :))}
                var uiImg = GetComponent<Image>();
                if (uiImg != null)
                {
                    if (ActorData.To.x != ActorData.From.x || ActorData.To.y != ActorData.From.y || ActorData.To.z != ActorData.From.z)
                    {
                        uiImg.color = ActorData.To;
                        LeanTween.color(mainWindow, ActorData.From, ActorData.Time).setDelay(ActorData.DelayTime).setLoopOnce().setEase(ActorData.TweenType).setOnComplete(() => { ActorData.IsActing = false; });
                    }

                }

                var canvasGroup = GetComponent<CanvasGroup>();
               // canvasGroup.alpha = ActorData.From.w;
                if (ActorData.To.w != ActorData.From.w)
                {
                    LeanTween.alphaCanvas(canvasGroup, ActorData.From.w, ActorData.Time).setDelay(ActorData.DelayTime).setLoopOnce().setEase(ActorData.TweenType).setOnComplete(() => {  ActorData.IsActing = false; });
                }
            }
        }
        #endregion
    }

    

}