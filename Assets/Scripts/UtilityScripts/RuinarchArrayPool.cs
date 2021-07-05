using System.Collections.Generic;
using Pathfinding.Util;

namespace UtilityScripts {
    public static class RuinarchArrayPool<T> {
        public static T[] Claim(int length) {
            T[] pool = ArrayPool<T>.Claim(length);
            return pool;
        }
        public static T[] ClaimWithExactLength(int length) {
            T[] pool = ArrayPool<T>.ClaimWithExactLength(length);
            return pool;
        }

        public static void Release(T[] array) {
            ArrayPool<T>.Release(ref array);
        }
        public static void ReleaseWithExactLength(T[] array) {
            ArrayPool<T>.Release(ref array, true);
        }
    }
}