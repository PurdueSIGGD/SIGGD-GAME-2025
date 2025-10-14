using UnityEngine;

/**
 * Singleton pattern implementation for MonoBehaviour classes.
 * Usage on singleton classes: "public class MyClass : Singleton<MyClass> { }"
 */
public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    public static T Instance // Public accessor for the singleton instance.
    {
        get
        {
            /* NOTE: the failsafe below will cause duplicate Singletons to load in when
               there are breakpoints in Awake(). Uncomment with caution. */

            //if (_instance == null)
            //{
            //    Debug.LogError("ERROR: trying to access Singleton that does not exist, creating new instance of " + typeof(T).Name);
            //    GameObject obj = new GameObject();
            //    obj.name = typeof(T).Name;
            //    obj.hideFlags = HideFlags.HideAndDontSave;
            //    _instance = obj.AddComponent<T>();
            //}

            return _instance;
        }
    }

    /**
     * <summary>
     * IMPORTANT: Call base.Awake() at the start of all overriden Awake() methods.
     * Ensures that only one instance of the singleton exists. If an instance already exists, the new one is destroyed.
     * </summary>
     */
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
        
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
