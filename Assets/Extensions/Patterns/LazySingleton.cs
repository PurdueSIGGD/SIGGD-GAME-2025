using System;

namespace Extensions.Patterns
{
    /**
     * <summary>
     * A thread-safe lazy singleton implementation for non-MonoBehaviour classes.
     * Ensures that only one instance of the class is created and provides global access to it.
     * Useful for manager or service classes that do not require Unity's MonoBehaviour features.
     * </summary>
     */
    public class LazySingleton<T> where T : class, new()
    {
        private static readonly Lazy<T> _instance = new Lazy<T>(() => new T());
        
        public static T Instance => _instance.Value;
    }
}