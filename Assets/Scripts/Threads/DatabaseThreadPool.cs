using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Data;
using System.Data.SQLite;
using System.Threading;

public class DatabaseThreadPool : BaseMonoBehaviour {
    public static DatabaseThreadPool Instance;

    private static readonly object THREAD_LOCKER = new object();

    private Queue<LogDatabaseThread> functionsToBeRunInThread;
    private Queue<LogDatabaseThread> functionsToBeResolved;

    private Thread newThread;
    private bool isRunning;

    void Awake() {
        Instance = this;
        isRunning = true;

        functionsToBeRunInThread = new Queue<LogDatabaseThread>();
        functionsToBeResolved = new Queue<LogDatabaseThread>();

        newThread = new Thread(RunThread) { IsBackground = true };
        newThread.Start();
    }
    protected override void OnDestroy() {
        isRunning = false;
        functionsToBeRunInThread.Clear();
        functionsToBeResolved.Clear();
        base.OnDestroy();
        Instance = null;
    }

    void LateUpdate() {
        if (functionsToBeResolved.Count > 0) {
            LogDatabaseThread action = functionsToBeResolved.Dequeue();
            action.FinishMultithread();
            ObjectPoolManager.Instance.ReturnLogDatabaseThreadToPool(action);
        }
    }
    public void AddToThreadPool(LogDatabaseThread multiThread) {
        functionsToBeRunInThread.Enqueue(multiThread);
    }

    private void RunThread() {
        while (isRunning) {
            if (functionsToBeRunInThread.Count > 0) {
                LogDatabaseThread newFunction = functionsToBeRunInThread.Dequeue();
                if (newFunction != null) {
                    lock (THREAD_LOCKER) {
                        newFunction.DoMultithread();
                    }
                }
                functionsToBeResolved.Enqueue(newFunction);
            }
        }
    }
}
