using System;
using System.Collections.Generic;

namespace OzricEngine.ext
{
    public static class DictionaryExt
    {
        /// <summary>
        /// Throw-safe get that returns null if the key doesn't exist.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <returns></returns>
        public static V Get<K,V>(this IDictionary<K,V> dictionary, K key) where V: class
        {
            if (!dictionary.ContainsKey(key))
                return null;

            return dictionary[key];
        }

        /// <summary>
        /// Return a value for a key, or create it using the given function.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="setter"></param>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <returns></returns>
        public static V GetOrSet<K,V>(this IDictionary<K,V> dictionary, K key, Func<V> setter) where V: class
        {
            if (!dictionary.ContainsKey(key))
                return dictionary[key] = setter();
    
            return dictionary[key];
        }
    }
}