using Object_Pools;
namespace Threads {
    public class SQLLogInsertThread : SQLWorkerItem {

        private Log _logToInsert;

        public void Initialize(Log p_log) {
            _logToInsert = LogPool.Claim();
            _logToInsert.Copy(p_log);
        }
        public override void Reset() {
            _logToInsert = null;
        }
        public override void DoMultithread() {
            base.DoMultithread();
            DatabaseManager.Instance.mainSQLDatabase.InsertLog(_logToInsert);
        }
        public override void FinishMultithread() {
            base.FinishMultithread();
            Messenger.Broadcast(UISignals.LOG_ADDED, _logToInsert);
        }
    }
}