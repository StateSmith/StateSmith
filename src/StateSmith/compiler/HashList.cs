using System.Collections.Generic;

namespace StateSmith.Compiling
{
    //supports 
    public class HashList<K, V>
    {
        private Dictionary<K, List<V>> dictionary = new Dictionary<K, List<V>>();

        public List<V> GetValues(K key)
        {
            return dictionary[key];
        }

        public List<V> GetValuesOrEmpty(K key)
        {
            if (dictionary.ContainsKey(key))
            {
                return GetValues(key);
            }
            return new List<V>();
        }

        public void AddIfMissing(K key, V value)
        {
            if (dictionary.ContainsKey(key))
            {
                List<V> list = dictionary[key];
                if (list.Contains(value) == false)
                {
                    list.Add(value);
                }
            }
            else
            {
                dictionary[key] = new List<V>()
                {
                    value
                };
            }
        }
    }
}
