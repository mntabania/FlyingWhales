using System.Collections.Generic;
using System;
using Pathfinding.Util;
using UnityEngine;

namespace UtilityScripts {
    public static class RuinarchListPool<T> {
        private static readonly List<List<T>> pool = new List<List<T>>();
        
        public static List<T> Claim () {
            lock (pool) {
                if (pool.Count > 0) {
                    List<T> ls = pool[0];
                    pool.RemoveAt(0);
                    return ls;
                }

                return new List<T>();
            }
        }
        
        public static void Release (List<T> list) {
            lock (pool) {
                if (!pool.Contains(list)) {
                    list.Clear();
                    pool.Add(list);
                } else {
#if DEBUG_LOG
                    Debug.LogError("Adding a list to pool but is already in list");
#endif
                }
            }
        }
    }

    public static class RuinarchCleanUpDictionaryPool {
        private static readonly List<Dictionary<string, WeakReference>> pool = new List<Dictionary<string, WeakReference>>();

        public static Dictionary<string, WeakReference> Claim() {
            lock (pool) {
                if (pool.Count > 0) {
                    Dictionary<string, WeakReference> ls = pool[0];
                    pool.RemoveAt(0);
                    return ls;
                }

                return new Dictionary<string, WeakReference>();
            }
        }

        public static void Release(Dictionary<string, WeakReference> dict) {
            lock (pool) {
                dict.Clear();
                pool.Add(dict);
            }
        }
    }
}