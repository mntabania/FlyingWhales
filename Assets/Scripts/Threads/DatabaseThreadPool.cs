using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using Threads;

public class DatabaseThreadPool : BaseMonoBehaviour {
    public static DatabaseThreadPool Instance;

    private static readonly object THREAD_LOCKER = new object();

    private Queue<SQLWorkerItem> functionsToBeRunInThread;
    private Queue<SQLWorkerItem> functionsToBeResolved;

    private Thread newThread;
    private bool isRunning;

    void Awake() {
        Instance = this;
        isRunning = true;

        functionsToBeRunInThread = new Queue<SQLWorkerItem>();
        functionsToBeResolved = new Queue<SQLWorkerItem>();

        //newThread = new Thread(RunThread) { IsBackground = true };
        //newThread.Start();
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
            SQLWorkerItem action = functionsToBeResolved.Dequeue();
            action.FinishMultithread();
            ObjectPoolManager.Instance.ReturnLogDatabaseThreadToPool(action);
        }
    }
    public void AddToThreadPool(SQLWorkerItem multiThread) {
        functionsToBeRunInThread.Enqueue(multiThread);
    }

    private void RunThread() {
        while (isRunning) {
            lock (THREAD_LOCKER) {
                if (functionsToBeRunInThread != null && functionsToBeRunInThread.Count > 0) {
                    SQLWorkerItem newFunction = functionsToBeRunInThread.Dequeue();
                    if (newFunction != null) {
                        newFunction.DoMultithread();
                    }
                    functionsToBeResolved.Enqueue(newFunction);
                }
            }
        }
    }
}
