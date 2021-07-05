using System.Collections.Generic;
using UnityEngine;
namespace Object_Pools {
    public static class LogPool {
        private static readonly List<Log> logPool = new List<Log>();

        public static void WarmUp(int p_amount) {
            lock (logPool) {
                for (int i = 0; i < p_amount; i++) {
                    logPool.Add(new Log());
                }    
            }
        }
        
        public static Log Claim() {
            lock (logPool) {
                if (logPool.Count > 0) {
                    Log log = logPool[0];
                    logPool.Remove(log);
                    return log;
                }
                return new Log();    
            }
        }
        public static void Release(Log p_log) {
            lock (logPool) {
                // Debug.Log($"Releasing log {p_log.rawText}");
                p_log.Reset();
                logPool.Add(p_log);    
            }
        }

        public static int GetCurrentLogsInPool() {
            return logPool.Count;
        }
    }
}