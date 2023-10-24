using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour { // T도 MonoBehaviour를 상속받는다
    private static T m_instance;
    public static T Instance{
        get{
            if(m_instance == null) {
                m_instance = FindObjectOfType<T>();
                if(m_instance == null) { // 그냥 존재도 안한다?!?! 그러면 만들어야지..
                    GameObject singleton = new GameObject(typeof(T).Name);
                    m_instance = singleton.GetComponent<T>();
                }
            }
            return m_instance;
        }
    }

    protected virtual void Awake() { // virtual : 상속해줄거야
        if(m_instance == null) {
            m_instance = this as T;
        }
        else {
            m_instance.transform.parent = null;
            Destroy(gameObject);
        }
            DontDestroyOnLoad(gameObject);
    }
}
