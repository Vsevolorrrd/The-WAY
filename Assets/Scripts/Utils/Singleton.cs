using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T m_Instance;
    protected bool isInstanced => m_Instance;
    
    public static T Instance
    {
        get
        {
            FindInstance();
            return m_Instance;
        }
    }
    
    protected static void FindInstance()
    {
        if (m_Instance == null)
        {
            m_Instance = (T)FindAnyObjectByType(typeof(T));
        }
    }
}
