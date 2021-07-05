using System.Collections.Generic;
namespace UtilityScripts {
    public class RuinarchHashSetPool<T> {
        private static readonly List<HashSet<T>> pool = new List<HashSet<T>>();
        
        public static HashSet<T> Claim () {
            lock (pool) {
                if (pool.Count > 0) {
                    HashSet<T> ls = pool[0];
                    pool.RemoveAt(0);
                    return ls;
                }

                return new HashSet<T>();
            }
        }
        
        public static void Release (HashSet<T> list) {
            lock (pool) {
                list.Clear();
                pool.Add(list);
            }
        }
    }
}