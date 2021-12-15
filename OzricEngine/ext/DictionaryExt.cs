using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OzricEngine.ext
{
    public static class DictionaryExt
    {
        public static V Get<K,V>(this IDictionary<K,V> dictionary, K key) where V: class
        {
            if (!dictionary.ContainsKey(key))
                return null;

            return dictionary[key];
        }

        public static V GetOrSet<K,V>(this IDictionary<K,V> dictionary, K key, Func<V> setter) where V: class
        {
            if (!dictionary.ContainsKey(key))
                return dictionary[key] = setter();
    
            return dictionary[key];
        }
    }
}