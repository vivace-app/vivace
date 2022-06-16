using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null) _instance = (T)FindObjectOfType(typeof(T));
            return _instance;
        }
    }

    protected void Awake() => CheckInstance();

    private void CheckInstance()
    {
        if (_instance == null)
        {
            _instance = (T)this;
            return;
        }

        if (Instance == this) return;

        Destroy(this);
    }
}