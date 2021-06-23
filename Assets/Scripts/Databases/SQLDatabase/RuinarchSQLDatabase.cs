using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Object_Pools;
using Threads;
using UnityEngine;
using UtilityScripts;
using Debug = UnityEngine.Debug;
namespace Databases.SQLDatabase {
    public class RuinarchSQLDatabase : IDisposable {

        private SQLiteConnection _dbConnection;

        public List<LOG_TAG> allLogTags { get; private set; }
        
        private const int LogRowLimit = 1500;
        private readonly string _bareBonesLogFields = "persistentID, date_tick, date_day, date_month, date_year, logText, category, key, file, involvedObjects, rawText";
        private readonly string _fullLogsFields;
        private readonly LOG_IDENTIFIER[] _allLogIdentifiersExceptNone;
        
        public RuinarchSQLDatabase() {
            allLogTags = CollectionUtilities.GetEnumValues<LOG_TAG>().ToList();
            for (int i = 0; i < allLogTags.Count; i++) {
                LOG_TAG logTag = allLogTags[i];
                _bareBonesLogFields = $"{_bareBonesLogFields}, {logTag.ToString()}";
            }
            _fullLogsFields = _bareBonesLogFields;
            
            List<LOG_IDENTIFIER> allIdentifiers = CollectionUtilities.GetEnumValues<LOG_IDENTIFIER>().ToList();
            allIdentifiers.Remove(LOG_IDENTIFIER.NONE);
            _allLogIdentifiersExceptNone = allIdentifiers.ToArray();
            for (int i = 0; i < _allLogIdentifiersExceptNone.Length; i++) {
                LOG_IDENTIFIER identifier = _allLogIdentifiersExceptNone[i];
                _fullLogsFields = $"{_fullLogsFields}, {identifier.ToString()}";
            }
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterNameUpdated);
        }
        
        #region Clean Up
        ~RuinarchSQLDatabase() {
            Dispose(false);
        }
        private void ReleaseUnmanagedResources() {
            //release unmanaged resources here
            CloseConnection();
            _dbConnection?.Dispose();
        }
        private void Dispose(bool disposing) {
            ReleaseUnmanagedResources();
            if (disposing) {
                //release managed resources here
                Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterNameUpdated);
            }
#if DEBUG_LOG
            Debug.Log("Ruinarch SQL database has been disposed.");
#endif
        }
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
#endregion

#region Initialization
        public void InitializeDatabase() {
            //This will either create or get the current gameDB database located at the Temp folder
            //so it is essential that if game came from save data, that it's relevant data be placed inside the Temp folder.
            OpenConnection();


            //create logs table
            SQLiteCommand command = _dbConnection.CreateCommand();
            string commandStr = $"CREATE TABLE IF NOT EXISTS 'Logs' (" +
                                $"'persistentID' STRING PRIMARY KEY UNIQUE, " +
                                $"'category' STRING NOT NULL, " +
                                $"'file' STRING NOT NULL, " +
                                $"'key' STRING NOT NULL, " +
                                $"'date_tick' INTEGER NOT NULL, " +
                                $"'date_day' INTEGER NOT NULL, " +
                                $"'date_month' INTEGER NOT NULL, " +
                                $"'date_year' INTEGER NOT NULL, " +
                                $"'logText' STRING NOT NULL, " +
                                $"'rawText' STRING NOT NULL COLLATE NOCASE, " +
                                $"'actionID' STRING, " +
                                $"'involvedObjects' STRING, " +
                                $"'isIntel' BOOLEAN DEFAULT false, ";
            LOG_IDENTIFIER[] logIdentifiers = CollectionUtilities.GetEnumValues<LOG_IDENTIFIER>();
            for (int i = 0; i < logIdentifiers.Length; i++) {
                LOG_IDENTIFIER identifier = logIdentifiers[i];
                if (identifier == LOG_IDENTIFIER.NONE) { continue; }
                commandStr = $"{commandStr}'{identifier.ToString()}' STRING, ";
            }
            LOG_TAG[] tags = CollectionUtilities.GetEnumValues<LOG_TAG>();
            for (int i = 0; i < tags.Length; i++) {
                LOG_TAG tag = tags[i];
                commandStr = $"{commandStr}'{tag.ToString()}' BOOLEAN DEFAULT false";
                if (i + 1 < tags.Length) {
                    commandStr = $"{commandStr}, ";
                }
            }
            commandStr = $"{commandStr});";//closing parenthesis
            command.CommandType = CommandType.Text;
            command.CommandText = commandStr;
            command.ExecuteNonQuery();

            List<string> existingColumns = new List<string>();
            commandStr = "PRAGMA table_info (Logs)";
            command.CommandText = commandStr;
            IDataReader dataReader = command.ExecuteReader();
            while (dataReader.Read()) {
                string columnName = dataReader.GetString(1);
                existingColumns.Add(columnName);
            }
            dataReader.Close();
            
            for (int i = 0; i < tags.Length; i++) {
                LOG_TAG tag = tags[i];
                if (!existingColumns.Contains(tag.ToString())) {
                    //create missing column
                    commandStr = $"ALTER TABLE Logs ADD COLUMN {tag.ToString()} BOOLEAN DEFAULT false";
                    command.CommandText = commandStr;
                    command.ExecuteNonQuery();    
                }
            }
        }
#endregion

#region Connection
        public void CloseConnection() {
            _dbConnection?.Close();
            _dbConnection?.Dispose();
            _dbConnection = null;
        }
        public void OpenConnection() {
            // dbConnection = new SQLiteConnection($@"Data Source={UtilityScripts.Utilities.gameSavePath}/Temp/gameDB.db;Version=3;");
            _dbConnection = new SQLiteConnection($@"Data Source=:memory:;");
            _dbConnection.Open();
            if (SaveManager.Instance.useSaveData) {
                LoadDatabaseFromFileToMemory($"{UtilityScripts.Utilities.gameSavePath}/Temp/gameDB.db");
            }
        }
        // public ConnectionState GetConnectionState() {
        //     return dbConnection.isD dbConnection.State;
        // }
#endregion

#region Logs
        public void InsertLogUsingMultiThread(Log log) {
            SQLLogInsertThread thread = ObjectPoolManager.Instance.CreateNewSQLInsertThread();
            thread.Initialize(log);
            MultiThreadPool.Instance.AddToThreadPool(thread);
        }
        public void InsertLog(Log log, out Log deletedLog) {
             log.FinalizeText();
            SQLiteCommand command = _dbConnection.CreateCommand();
            //Need to replace single quotes in log message to two single quotes to prevent SQL command errors
            //Reference: https://stackoverflow.com/questions/603572/escape-single-quote-character-for-use-in-an-sqlite-query
            string replacedLog = log.logText.Replace("'", "''");
            string replacedRawText = log.rawText.Replace("'", "''");
            string insertStr = "INSERT OR REPLACE INTO 'Logs'('persistentID', 'category', 'file', 'key', 'date_tick', 'date_day', 'date_month', 'date_year', 'logText', 'rawText', 'actionID', 'involvedObjects'";
            if (log.fillers.Count > 0) {
                for (int i = 0; i < log.fillers.Count; i++) {
                    LogFillerStruct filler = log.fillers[i];
                    insertStr = $"{insertStr}, '{filler.identifier.ToString()}'";
                }    
            } 
            if (log.tags.Count > 0) {
                for (int i = 0; i < log.tags.Count; i++) {
                    LOG_TAG tag = log.tags[i];
                    insertStr = $"{insertStr}, '{tag.ToString()}'";
                    if (i+1 == log.tags.Count) {
                        insertStr = $"{insertStr})"; //closing parenthesis last tag
                    }
                }    
            } else {
                insertStr = $"{insertStr})"; //closing parenthesis if no tags were provided
            }
            
            string valuesStr = $"VALUES ('{log.persistentID}', '{log.category}', '{log.file}', '{log.key}', '{log.gameDate.tick.ToString()}', '{log.gameDate.day.ToString()}', '{log.gameDate.month.ToString()}', '{log.gameDate.year.ToString()}', '{replacedLog}', '{replacedRawText}', '{log.actionID}', '{log.allInvolvedObjectIDs}'";
            if (log.fillers.Count > 0) {
                for (int i = 0; i < log.fillers.Count; i++) {
                    LogFillerStruct filler = log.fillers[i];
                    valuesStr = $"{valuesStr}, '{filler.GetSQLText()}'";
                }    
            } 
            if (log.tags.Count > 0) {
                for (int i = 0; i < log.tags.Count; i++) {
                    valuesStr = $"{valuesStr}, '1'"; //switch tag to true
                    if (i+1 == log.tags.Count) {
                        valuesStr = $"{valuesStr})"; //closing parenthesis last tag
                    }
                }    
            } else {
                valuesStr = $"{valuesStr})"; //closing parenthesis if no tags were provided
            }
            string commandStr = $"{insertStr} {valuesStr}";
            
            command.CommandType = CommandType.Text;
            command.CommandText = commandStr;
            command.ExecuteNonQuery();

            //check if row limit has been reached, if so then delete oldest log that is not an intel.
            command.CommandText = $"SELECT COUNT(*) FROM 'Logs'";
            IDataReader dataReader = command.ExecuteReader();
            int rowCount = 0;
            while (dataReader.Read()) {
                rowCount = dataReader.GetInt32(0);
            }
            dataReader.Close();
            deletedLog = null;
            if (rowCount > LogRowLimit) {
                //row limit has been reached, will delete oldest entry
                deletedLog = DeleteOldestLog();
            }
            command.Dispose();
        }
        public List<Log> GetLogsThatMatchCriteria(string persistentID, string textLike, List<LOG_TAG> tags, int limit = -1) {
#if UNITY_EDITOR
            Stopwatch timer = new Stopwatch();
            timer.Start();
#endif
            if (tags.Count == 0) {
                return null;
            }
            //if no tags were passed then default to use all tags instead. Since we found it weird that if not filters were checked it would result in no logs being shown.
            var tagsToUse = tags;//tags.Count > 0 ? tags : allLogTags;
            
            SQLiteCommand command = _dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;

            string commandStr = $"SELECT {_bareBonesLogFields} FROM Logs WHERE involvedObjects LIKE '%{persistentID}%'";
            //append string search condition
            if (!string.IsNullOrEmpty(textLike)) {
                textLike = textLike.Replace("'", "''");
                commandStr = $"{commandStr} AND rawText LIKE '%{textLike}%'";
            }
            //append tags condition.
            if (tagsToUse.Count > 0) {
                commandStr = $"{commandStr} AND (";
                for (int i = 0; i < tagsToUse.Count; i++) {
                    LOG_TAG tag = tagsToUse[i];
                    commandStr = $"{commandStr} {tag.ToString()} = '1'";
                    if (i + 1 < tagsToUse.Count) {
                        commandStr = $"{commandStr} OR";
                    }else if (i + 1 == tagsToUse.Count) {
                        commandStr = $"{commandStr})";
                    }
                }
            }
            commandStr = $"{commandStr} ORDER BY date_year ASC, date_month ASC, date_day ASC, date_tick ASC";
            if (limit != -1) {
                commandStr = $"{commandStr} LIMIT {limit.ToString()}";
            }
            // Debug.Log($"Trying to get logs that match criteria, full query command is {commandStr}");
            command.CommandText = commandStr;
            IDataReader dataReader = command.ExecuteReader();
            List<Log> logs = RuinarchListPool<Log>.Claim();
            while (dataReader.Read()) {
                Log log = ConvertToBareBonesLog(dataReader);
                logs.Add(log);
            }
            dataReader.Close();
#if UNITY_EDITOR
            timer.Stop();
            // Debug.Log($"Total log query time was {timer.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds");
#endif
            return logs;    
        }
        public List<string> GetLogIDsThatMatchCriteria(List<string> pool, string textLike, List<LOG_TAG> tags, int limit = -1) {
#if UNITY_EDITOR
            Stopwatch timer = new Stopwatch();
            timer.Start();
#endif
            if (tags.Count == 0) {
                return null;
            }
            //if no tags were passed then default to use all tags instead. Since we found it weird that if not filters were checked it would result in no logs being shown.
            var tagsToUse = tags;//tags.Count > 0 ? tags : allLogTags;
            
            SQLiteCommand command = _dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;

            string commandStr = $"SELECT persistentID FROM Logs WHERE(";
            for (int i = 0; i < pool.Count; i++) {
                string idFromPool = pool[i];
                commandStr = $"{commandStr}persistentID = '{idFromPool}'";
                if (i + 1 < pool.Count) {
                    commandStr = $"{commandStr} OR ";    
                }
            }
            commandStr = $"{commandStr})";
            
            //append string search condition
            if (!string.IsNullOrEmpty(textLike)) {
                textLike = textLike.Replace("'", "''");
                commandStr = $"{commandStr} AND rawText LIKE '%{textLike}%'";
            }
            
            //append tags condition.
            if (tagsToUse.Count > 0) {
                commandStr = $"{commandStr} AND (";
                for (int i = 0; i < tagsToUse.Count; i++) {
                    LOG_TAG tag = tagsToUse[i];
                    commandStr = $"{commandStr} {tag.ToString()} = '1'";
                    if (i + 1 < tagsToUse.Count) {
                        commandStr = $"{commandStr} OR";
                    }else if (i + 1 == tagsToUse.Count) {
                        commandStr = $"{commandStr})";
                    }
                }
            }
            commandStr = $"{commandStr} ORDER BY date_year DESC, date_month DESC, date_day DESC, date_tick DESC";
            if (limit != -1) {
                commandStr = $"{commandStr} LIMIT {limit.ToString()}";
            }
            // Debug.Log($"Trying to get notifications that match criteria, full query command is {commandStr}");
            command.CommandText = commandStr;
            IDataReader dataReader = command.ExecuteReader();
            List<string> logs = new List<string>();
            while (dataReader.Read()) {
                string id = dataReader.GetString(0);
                logs.Add(id);
            }
            dataReader.Close();
#if UNITY_EDITOR
            timer.Stop();
            // Debug.Log($"Total notification query time was {timer.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds");
#endif
            return logs;    
        }
        public Log GetLogWithPersistentID(string persistentID) {
            SQLiteCommand command = _dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = $"SELECT {_bareBonesLogFields} FROM Logs WHERE persistentID = '{persistentID}'";
            IDataReader dataReader = command.ExecuteReader();
            while (dataReader.Read()) {
                return ConvertToBareBonesLog(dataReader);
            }
            dataReader.Close();
            return default;
        }
        public Log GetFullLogWithPersistentID(string persistentID) {
            SQLiteCommand command = _dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = $"SELECT {_fullLogsFields} FROM Logs WHERE persistentID = '{persistentID}'";
            IDataReader dataReader = command.ExecuteReader();
            while (dataReader.Read()) {
                return ConvertToFullLog(dataReader);
            }
            dataReader.Close();
            return default;
        }
        public void SetLogIntelState(string persistentID, bool isIntel) {
            SQLiteCommand command = _dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = $"UPDATE 'Logs' SET isIntel = {isIntel.ToString()} WHERE persistentID = '{persistentID}'";
            command.ExecuteNonQuery();
            command.Dispose();
            // Debug.Log($"Set intel state of log {persistentID} to {isIntel.ToString()}");
        }
        public void UpdateInvolvedObjects(in Log log) {
            SQLiteCommand command = _dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = $"UPDATE 'Logs' SET involvedObjects = '{log.allInvolvedObjectIDs}' WHERE persistentID = '{log.persistentID}'";
            command.ExecuteNonQuery();
            command.Dispose();
            Messenger.Broadcast(UISignals.LOG_IN_DATABASE_UPDATED, log);
            // Debug.Log($"Set involved objects of log {log.persistentID} to {log.allInvolvedObjectIDs}");
        }
        private Log DeleteOldestLog() {
            SQLiteCommand command = _dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT persistentID FROM Logs ORDER BY rowid LIMIT 1"; //WHERE isIntel = 0
            IDataReader dataReader = command.ExecuteReader();
            string logIDToDelete = string.Empty;
            while (dataReader.Read()) {
                logIDToDelete = dataReader.GetString(0);
            }
            dataReader.Close();
            command.Dispose();
            if (!string.IsNullOrEmpty(logIDToDelete)) {
                return DeleteLog(logIDToDelete);
            }
            return null;
        }
        private Log DeleteLog(string persistentID) {
            Log deletedLog = GetLogWithPersistentID(persistentID);
            // Debug.Log($"Will delete log with ID: {persistentID}");
            SQLiteCommand command = _dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = $"DELETE FROM Logs WHERE persistentID = '{persistentID}'";
            command.ExecuteNonQuery();
            command.Dispose();
            // LogPool.Release(deletedLog);
            return deletedLog;
        }
        public List<Log> GetFullLogsMentioning(string persistentID) {
            SQLiteCommand command = _dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            string commandStr = $"SELECT {_fullLogsFields} FROM Logs WHERE involvedObjects LIKE '%{persistentID}%'";
            command.CommandText = commandStr;
            IDataReader dataReader = command.ExecuteReader();
            List<Log> logs = RuinarchListPool<Log>.Claim();
            while (dataReader.Read()) {
                Log log = ConvertToFullLog(dataReader);
                logs.Add(log);
            }
            return logs;
        }
        private void OnCharacterNameUpdated(Character character) {
            UpdateCharacterNameThread databaseThread = ObjectPoolManager.Instance.CreateNewLogDatabaseThread();
            databaseThread.Initialize(character);
            MultiThreadPool.Instance.AddToThreadPool(databaseThread);
        }
#endregion

#region Utilities
        private Log ConvertToBareBonesLog(IDataReader dataReader) {
            string id = dataReader.GetString(0);
            int tick = dataReader.GetInt32(1);
            int day = dataReader.GetInt32(2);
            int month = dataReader.GetInt32(3);
            int year = dataReader.GetInt32(4);
            string logText = dataReader.GetString(5);
            string category = dataReader.GetString(6);
            string key = dataReader.GetString(7);
            string file = dataReader.GetString(8);
            string involvedObjects = dataReader.GetString(9);
            string rawText = dataReader.GetString(10);
            
            int currentColumnIndex = 11;
            List<LOG_TAG> logTags = new List<LOG_TAG>();
            for (int i = 0; i < allLogTags.Count; i++) {
                LOG_TAG currentType = allLogTags[i];
                bool isTagOn = dataReader.GetBoolean(currentColumnIndex);
                if (isTagOn) {
                    logTags.Add(currentType);
                }
                currentColumnIndex++;
            }
            return GameManager.CreateNewLog(id, new GameDate(month, day, year, tick), logText, category, key, file, involvedObjects, logTags, rawText);
        }
        /// <summary>
        /// Convert a database entry into a full log.
        /// NOTE: By full log meaning everything included in a bare bones log as well as all the log fillers.
        /// </summary>
        /// <param name="dataReader">The database stream.</param>
        /// <returns>The fully converted log.</returns>
        private Log ConvertToFullLog(IDataReader dataReader) {
            string id = dataReader.GetString(0);
            int tick = dataReader.GetInt32(1);
            int day = dataReader.GetInt32(2);
            int month = dataReader.GetInt32(3);
            int year = dataReader.GetInt32(4);
            string logText = dataReader.GetString(5);
            string category = dataReader.GetString(6);
            string key = dataReader.GetString(7);
            string file = dataReader.GetString(8);
            string involvedObjects = dataReader.GetString(9);
            string rawText = dataReader.GetString(10);
            
            int currentColumnIndex = 11;
            List<LOG_TAG> logTags = new List<LOG_TAG>();
            for (int i = 0; i < allLogTags.Count; i++) {
                LOG_TAG currentType = allLogTags[i];
                bool isTagOn = dataReader.GetBoolean(currentColumnIndex);
                if (isTagOn) {
                    logTags.Add(currentType);
                }
                currentColumnIndex++;
            }
            
            //fillers
            List<LogFillerStruct> logFillers = new List<LogFillerStruct>();
            for (int i = 0; i < _allLogIdentifiersExceptNone.Length; i++) {
                LOG_IDENTIFIER identifier = _allLogIdentifiersExceptNone[i];
                if (!dataReader.IsDBNull(currentColumnIndex)) {
                    object obj = dataReader.GetValue(currentColumnIndex);
                    string rawFillerValue = string.Empty;
                    if (obj is int number) {
                        rawFillerValue = number.ToString();
                    } else if (obj is string str) {
                        rawFillerValue = str;
                    }
                    if (!string.IsNullOrEmpty(rawFillerValue)) {
                        LogFillerStruct logFillerStruct = new LogFillerStruct(rawFillerValue, identifier);
                        logFillers.Add(logFillerStruct);
                    }

                }
                currentColumnIndex++;
            }
            return GameManager.CreateNewLog(id, new GameDate(month, day, year, tick), logText, category, key, file, involvedObjects, logTags, rawText, logFillers);
        }
        public void SaveInMemoryDatabaseToFile(string filePath) {
            using (SQLiteConnection databaseInFile = new SQLiteConnection($"Data Source={filePath};Version=3;")) {
                databaseInFile.Open();
                _dbConnection.BackupDatabase(databaseInFile, "main", "main", -1, null, -1);
            }
        }
        public void LoadDatabaseFromFileToMemory(string filePath) {
            using (SQLiteConnection databaseInFile = new SQLiteConnection($"Data Source={filePath};Version=3;")) {
                databaseInFile.Open();
                databaseInFile.BackupDatabase(_dbConnection, "main", "main", -1, null, -1);
            }
        }
#endregion

#region Plugins
        // static Constructor
        static RuinarchSQLDatabase() {
            var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
#if UNITY_EDITOR_32
            var dllPath = Application.dataPath + "/Plugins"/x86";
#elif UNITY_EDITOR_64
            var dllPath = Application.dataPath + "/Plugins/x86_64";
#else // Player
            var dllPath = Application.dataPath + "/Plugins";
#endif
            if (currentPath != null && currentPath.Contains(dllPath) == false)
                Environment.SetEnvironmentVariable("PATH", currentPath + "/" + dllPath, EnvironmentVariableTarget.Process); //Path.PathSeparator
        }
#endregion
    }
}