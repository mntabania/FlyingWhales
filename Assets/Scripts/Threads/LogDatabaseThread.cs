using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class LogDatabaseThread : Multithread {

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
    public void Reset() {
        _character = null;
    }
    
    private void LogDatabaseUpdateForCharacter(Character character) {
        List<Log> logs = DatabaseManager.Instance.mainSQLDatabase.GetFullLogsMentioning(character.persistentID);
        for (int i = 0; i < logs.Count; i++) {
            Log log = logs[i];
            log.ReEvaluateWholeText();
            DatabaseManager.Instance.mainSQLDatabase.InsertLog(log);
        }
        logs.ReleaseLogInstancesAndLogList();
    }
}
