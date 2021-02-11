﻿using System.Collections.Generic;
using Pathfinding.Util;
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
                list.Clear();
                pool.Add(list);
            }
        }
    }
}