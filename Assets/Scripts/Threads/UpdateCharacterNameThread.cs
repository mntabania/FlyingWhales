using System.Collections.Generic;
using UtilityScripts;

namespace Threads {
    /// <summary>
    /// Multi-threaded script to update the name of a character in the SQL Database. 
    /// </summary>
    public class UpdateCharacterNameThread : SQLWorkerItem {

        private Character _character;
    
        public void Initialize(Character character) {
            _character = character;
        }
        public override void DoMultithread() {
            base.DoMultithread();
            LogDatabaseUpdateForCharacter(_character);
        }
        public override void FinishMultithread() {
            base.FinishMultithread();
            Messenger.Broadcast(UISignals.LOG_MENTIONING_CHARACTER_UPDATED, _character);
        }
        public override void Reset() {
            _character = null;
        }
    
        private void LogDatabaseUpdateForCharacter(Character character) {
            List<Log> logs = DatabaseManager.Instance.mainSQLDatabase.GetFullLogsMentioning(character.persistentID);
            for (int i = 0; i < logs.Count; i++) {
                Log log = logs[i];
                log.ReEvaluateWholeText();
                DatabaseManager.Instance.mainSQLDatabase.InsertLog(log, out var deletedLog);
            }
            logs.ReleaseLogInstancesAndLogList();
        }
    }
}
