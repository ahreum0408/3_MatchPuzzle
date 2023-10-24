using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour { // T�� MonoBehaviour�� ��ӹ޴´�
    private static T m_instance;
    public static T Instance{
        get{
            if(m_instance == null) {
                m_instance = FindObjectOfType<T>();
                if(m_instance == null) { // �׳� ���絵 ���Ѵ�?!?! �׷��� ��������..
                    GameObject singleton = new GameObject(typeof(T).Name);
                    m_instance = singleton.GetComponent<T>();
                }
            }
            return m_instance;
        }
    }

    protected virtual void Awake() { // virtual : ������ٰž�
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
