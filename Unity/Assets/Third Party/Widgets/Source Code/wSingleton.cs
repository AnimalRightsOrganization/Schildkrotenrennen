using System.Collections;
using UnityEngine;

/// <summary>
/// This class does not create multiple instances on re-loading same level
/// </summary>
/// <typeparam name="T">Type of Class</typeparam>
public class wSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T m_instance = null;

    public static T Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = GameObject.FindObjectOfType<T>();

                //Tell unity not to destroy this object when loading a new scene!
                DontDestroyOnLoad(m_instance.gameObject);
            }
            return m_instance;
        }
    }

    public virtual void Awake()
    {
        if (m_instance == null)
        {
            //If I am the first instance, make me the Singleton
            m_instance = this as T;
            DontDestroyOnLoad(this);
            //Debug.Log("Singelton Created...");
        }
        else
        {
            //If a Singleton already exists and you find
            //another reference in scene, destroy it!
            if (this != m_instance)
                Destroy(this.gameObject);
        }
    }
}
