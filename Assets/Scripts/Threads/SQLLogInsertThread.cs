using Object_Pools;
namespace Threads {
    public class SQLLogInsertThread : SQLWorkerItem {

        private Log _logToInsert;
        private Log _deletedLog;

        public void Initialize(Log p_log) {
            _logToInsert = LogPool.Claim();
            _logToInsert.Copy(p_log);
        }
        public override void Reset() {
            _logToInsert = null;
        }
        public override void DoMultithread() {
            base.DoMultithread();
            DatabaseManager.Instance.mainSQLDatabase.InsertLog(_logToInsert, out _deletedLog);
        }
        public override void FinishMultithread() {
            base.FinishMultithread();
            Messenger.Broadcast(UISignals.LOG_ADDED, _logToInsert);
            if (_deletedLog != null) {
                Messenger.Broadcast(UISignals.LOG_REMOVED_FROM_DATABASE, _deletedLog);    
            }
        }
    }
}