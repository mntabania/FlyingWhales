using System.Collections.Generic;
using Inner_Maps.Location_Structures;

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
        Messenger.Broadcast(Signals.LOG_MENTIONING_CHARACTER_UPDATED, _character);
    }
    public void Reset() {
        _character = null;
    }
    
    private void LogDatabaseUpdateForCharacter(Character character) {
        List<Log> logs = DatabaseManager.Instance.mainSQLDatabase.GetFullLogsMentioning(character.persistentID);
        for (int i = 0; i < logs.Count; i++) {
            Log log = logs[i];
            for (int j = 0; j < log.fillers.Count; j++) {
                LogFillerStruct logFiller = log.fillers[j];
                if (logFiller.type != null && (logFiller.type == typeof(LocationStructure) || logFiller.type.IsSubclassOf(typeof(LocationStructure)))) {
                    continue; //Do not update structure names because it is okay for them to be inaccurate. 
                }
                logFiller.ForceUpdateValueBasedOnConnectedObject();
                log.fillers[j] = logFiller;
            }
            log.ResetText();
            log.FinalizeText();
            DatabaseManager.Instance.mainSQLDatabase.InsertLog(log);
        }
    }
}
