using UnityEngine;

namespace HotFix
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Get;

        void Awake()
        {
            Get = this;
        }
    }
}