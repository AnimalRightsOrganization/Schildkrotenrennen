

namespace eeGames.Widget
{
    public interface IWidget
    {
        /// <summary>
        /// Widget Id (name)
        /// </summary>
        WidgetName Id { get; set; }
        /// <summary>
        /// Tells if Widget is visible or not
        /// </summary>
        bool IsActive { get; set; }
        /// <summary>
        /// Don't destroy Widget once it gets load.
        /// </summary>
        bool IsPoolOnLoad { get; set; }
        /// <summary>
        /// Tween Componet Contains Position | Scale | Rotation Tweens
        /// </summary>
        Tweens Tweens { get; set; }

        /// <summary>
        /// Gets called before Show , only get called when Widget Pushed.
        /// </summary>
        void Init();
        /// <summary>
        /// Gets called When Widget Pushed.
        /// </summary>
        /// <param name="IsPlayTween"> Play OnHide Tween of Current Widget </param>
        void Show(  bool IsPlayTween = true);
        /// <summary>
        /// Gets called when Widget poped
        /// </summary>
        /// <param name="IsPlayTween"> Play OnShow Tween of previous widget </param>
        void Hide(bool IsPlayTween = true);
        /// <summary>
        /// Explicitly called this function and do update logic in it.
        /// </summary>
        void UpdateWidget();
        /// <summary>
        /// Destroy Widget(gameobject)
        /// </summary>
        void DestroyWidget();

    }

    [System.Serializable]
    public class WidgetEvent : UnityEngine.Events.UnityEvent
    { }
}
