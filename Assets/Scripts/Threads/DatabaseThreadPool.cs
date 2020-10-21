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

    private Queue<string> functionsToBeRunInThread;
    // private Queue<string> functionsToBeResolved;

    private Thread newThread;
    //private ManualResetEventSlim exitHandle = new ManualResetEventSlim();
    private bool isRunning;

    void Awake() {
        Instance = this;
        this.isRunning = true;

        functionsToBeRunInThread = new Queue<string>();
        // functionsToBeResolved = new Queue<string>();

        // newThread = new Thread(RunThread);
        // newThread.IsBackground = true;
        // newThread.Start();
    }
    protected override void OnDestroy() {
        this.isRunning = false;
        functionsToBeRunInThread.Clear();
        // functionsToBeResolved.Clear();
        base.OnDestroy();
        Instance = null;
    }

    // void LateUpdate() {
    //     if (this.functionsToBeResolved.Count > 0) {
    //         string action = this.functionsToBeResolved.Dequeue();
    //         
    //     }
    // }

    public void AddToThreadPool(string multiThread) {
        functionsToBeRunInThread.Enqueue(multiThread);
    }

    private void RunThread() {
        // while (isRunning) { // && !exitHandle.Wait(20)
        //     if (this.functionsToBeRunInThread.Count > 0) {
        //         //Thread.Sleep(20);
        //         string newFunction = this.functionsToBeRunInThread.Dequeue();
        //         if (newFunction != null) {
        //             lock (THREAD_LOCKER) {
        //                 if (DatabaseManager.Instance != null && DatabaseManager.Instance.mainSQLDatabase != null) {
        //                     DatabaseManager.Instance.mainSQLDatabase.ExecuteInsertCommand(newFunction);
        //                 }
        //             }
        //             // this.functionsToBeResolved.Enqueue(newFunction);
        //         }
        //     }
        // }
    }
    private void Stop() {
        ////exitHandle.Set();
        //exitHandle.Dispose();
        //exitHandle = null;
        newThread.Join();
    }
    // void OnDestroy() {
    //     this.isRunning = false;
    //     //Stop();
    // }
}
