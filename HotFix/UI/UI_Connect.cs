using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

namespace HotFix
{
    public class UI_Connect : UIBase
    {
        public Transform m_Rotate;

        void Awake()
        {
            m_Rotate = transform.Find("Mask/Rotate");
            var sa = ResManager.LoadSpriteAtlas("Atlas/Items");
            m_Rotate.GetComponent<Image>().sprite = sa.GetSprite("SettingsShadow");
        }

        void OnEnable()
        {
            SpriteAtlasManager.atlasRequested += RequestAtlas;
        }

        void OnDisable()
        {
            SpriteAtlasManager.atlasRequested -= RequestAtlas;
        }

        void Update()
        {
            m_Rotate.Rotate(Vector3.back, 10);
        }

        SpriteAtlas spriteAtlas;
        void RequestAtlas(string tag, System.Action<SpriteAtlas> callback)
        {
            if (spriteAtlas == null)
            {
                spriteAtlas = ResManager.LoadSpriteAtlas("Atlas/Items");
            }
            callback(spriteAtlas);
        }
    }
}