using System.Collections.Generic;
using Object_Pools;
namespace UtilityScripts {
    public static class LogUtilities {

        public static LOG_TAG[] Break_Up_Tags = new[] {LOG_TAG.Social, LOG_TAG.Life_Changes};
        public static LOG_TAG[] Become_Cannibal_Tags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Needs, LOG_TAG.Crimes};
        public static LOG_TAG[] Evangelize_Tags = new[] {LOG_TAG.Crimes, LOG_TAG.Work, LOG_TAG.Social};
        public static LOG_TAG[] Criminal_Tags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Crimes};
        public static LOG_TAG[] Declare_Wanted_Tags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Crimes, LOG_TAG.Major};
        public static LOG_TAG[] Life_Changes_Crimes_Tags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Crimes};
        public static LOG_TAG[] Party_Quest_Tags = new[] {LOG_TAG.Party, LOG_TAG.Major};
        public static LOG_TAG[] Player_Life_Changes_Tags = new[] {LOG_TAG.Player, LOG_TAG.Life_Changes};
        public static LOG_TAG[] Agitate_Tags = new[] {LOG_TAG.Player, LOG_TAG.Combat};
        public static LOG_TAG[] Cultist_Instruct_Tags = new[] {LOG_TAG.Player, LOG_TAG.Crimes};
        public static LOG_TAG[] Social_Life_Changes_Tags = new[] {LOG_TAG.Social, LOG_TAG.Life_Changes};

        public static void ReleaseLogInstancesAndLogList(this List<Log> p_logs) {
            for (int i = 0; i < p_logs.Count; i++) {
                Log log = p_logs[i];
                LogPool.Release(log);
            }
            RuinarchListPool<Log>.Release(p_logs);
        }
    }
}