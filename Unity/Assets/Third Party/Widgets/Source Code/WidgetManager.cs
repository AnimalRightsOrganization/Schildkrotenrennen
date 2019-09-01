using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace eeGames.Widget
{
    public class WidgetManager : wSingleton<WidgetManager>
    {
        // Find parent automatically based on tag
        [SerializeField]
        private Transform m_parent;
        [SerializeField]
        private List<Widget> m_stack;              // Holds Currently Active UI's
        [SerializeField]
        private List<Widget> m_pooledWidgets;

        public override void Awake()
        {
            base.Awake();
            m_parent = GameObject.FindGameObjectWithTag("Parent").transform;   // assign parent tag to canvas or panel which you want acts as parent
            if (m_parent == null) Debug.LogError("<color=red>Create new tag Parent & assign it to Canvas(parent):</color>");
            else m_stack = new List<Widget>();
        }

        #region Utility Methods
        LTDescr m_handler = null;
        /// <summary>
        /// Remove Widget from top of Stack
        /// </summary>
        /// <param name="isPlayShowTween">Play OnShow Tween of Previous Widget</param>
        public void Pop(bool isPlayShowTween = true)
        {
            if (m_stack.Count < 1)
            {
                Debug.Log("<color=red>There is no more Widget in stack:</color>");
                return;
            }


            if (m_stack.Count > 1)      // if there are more than 1 widget
                m_stack[1].Show(isPlayShowTween);

            Widget top = m_stack[0];
            top.Hide();
            if (!top.IsPoolOnLoad)
            {
                if (top.Tweens != null)
                    m_handler = top.Tweens.GetTweenHandler();
                if (m_handler != null)
                {
                    m_handler.setOnComplete(() =>
                    {
                        m_pooledWidgets.Remove(top);
                        top.DestroyWidget();
                    });
                }
                else
                {
                    m_pooledWidgets.Remove(top);
                    top.DestroyWidget();
                }


            }
            m_stack.RemoveAt(0);
        }


        /// <summary>
        /// Hide Widget by Id to voilate LIFO Rule, its Handy in Some Situations
        /// </summary>
        /// <param name="id"> Widget to Hide </param>
        /// <param name="isPlayShowTween">Play OnShow Tween of Previous Widget</param>
        public void Pop(WidgetName id, bool isPlayShowTween = true)
        {
            if (m_stack.Count < 1)
            {
                Debug.Log("<color=red>There is no more item in stack:</color>");
                return;
            }


            if (m_stack.Count > 1)      // if there are more than 1 widget
                m_stack[1].Show(isPlayShowTween);

            Widget top = GetWidget(id);
            if (top == null)
            {
                Debug.Log("<color=red>There is no Active Widget by this Id:</color>" + " " + id.ToString());
                return;
            }
            top.Hide();
            if (!top.IsPoolOnLoad)
            {
                if (top.Tweens != null)
                    m_handler = top.Tweens.GetTweenHandler();
                if (m_handler != null)
                {
                    m_handler.setOnComplete(() =>
                    {
                        m_pooledWidgets.Remove(top);
                        Destroy(top.gameObject);
                    });
                }
                else
                {
                    m_pooledWidgets.Remove(top);
                    Destroy(top.gameObject);
                }


            }
            m_stack.Remove(top);
        }

      /// <summary>
      /// Push Widget on top of stack
      /// </summary>
      /// <param name="id">Id of Widget</param>
      /// <param name="isPlayHideTween">Play hide tween of previous widget</param>
      /// <param name="lastActive">Is previous Widget Visible</param>
      /// <param name="lastInteractive">Is previous Widget Interactable, in case of small panels</param>
      /// <param name="firstChild">Set Widget as first child </param>
        public void Push(WidgetName id, bool isPlayHideTween = true, bool lastActive = false, bool lastInteractive = false, bool firstChild = false)
        {

            if (m_stack.Count >= 1) 
            { 
                if (!lastActive) { m_stack[0].Hide(isPlayHideTween); }
                m_stack[0].CanvasGroup.interactable = lastInteractive; 
            }

            Widget widget = GetPooledWidget(id);

            if (!widget || !widget.IsPoolOnLoad)
            {
                var wToLoad = Database.GetDatabase().Find(w => w.Id == id);
                GameObject widgetObj = Instantiate(Resources.Load(wToLoad.perfabPath)) as GameObject;
                widget = widgetObj.GetComponent<Widget>();
                widget.IsPoolOnLoad = wToLoad.PoolOnLoad;
                widget.Id = id;
                m_pooledWidgets.Add(widget);

            }

            widget.transform.SetParent(m_parent, false);

            if (firstChild) widget.transform.SetAsFirstSibling();
            else widget.transform.SetAsLastSibling();
            widget.Init();
            widget.Show();
            m_stack.Insert(0, widget);

        }

        /// <summary>
        /// Delete All Widgets, Even deletes pooled widgets
        /// </summary>
        public void UnWindStack()
        {
            for (int i = 0; i < m_pooledWidgets.Count; i++ )
            {
                m_pooledWidgets[i].DestroyWidget();
            }
            m_stack.Clear();
            m_pooledWidgets.Clear();
        }

        /// <summary>
        /// Update Widget by Id
        /// </summary>
        /// <param name="name">Id of Widget</param>
        public void UpdateWidget(WidgetName name)
        {
            Widget widget = GetWidget(name);
            if (widget) widget.UpdateWidget();
        }

        /// <summary>
        /// Get Widget by Id
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Widget GetWidget(WidgetName name)
        {
            return m_stack.Find(id => id.Id == name);

        }
        #endregion

        /// <summary>
        /// These Methods used by this class no concern for user
        /// </summary>
        
        #region Helper Methods 

        private Widget GetPooledWidget(WidgetName name)
        {
            return m_pooledWidgets.Find(id => id.Id == name);

        }

        private WidgetDatabase m_database;
        private WidgetDatabase Database
        {
            get
            {
                if (m_database == null)
                {
                    m_database = Resources.Load<WidgetDatabase>("WidgetDatabase");
                }
                return m_database;
            }
        }
        #endregion
    }
}