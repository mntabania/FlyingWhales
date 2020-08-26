using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public class MultiThreadPool : BaseMonoBehaviour {
    public static MultiThreadPool Instance;

    private static readonly object THREAD_LOCKER = new object();

    private Queue<Multithread> functionsToBeRunInThread;
    private Queue<Multithread> functionsToBeResolved;

    private Thread newThread;
    //private ManualResetEventSlim exitHandle = new ManualResetEventSlim();
    private bool isRunning;

    void Awake() {
        Instance = this;
        this.isRunning = true;

        functionsToBeRunInThread = new Queue<Multithread>();
        functionsToBeResolved = new Queue<Multithread>();

        newThread = new Thread(RunThread);
        newThread.IsBackground = true;
        newThread.Start();
    }
    protected override void OnDestroy() {
        this.isRunning = false;
        functionsToBeRunInThread.Clear();
        functionsToBeResolved.Clear();
        base.OnDestroy();
        Instance = null;
    }

    void LateUpdate() {
        if (this.functionsToBeResolved.Count > 0) {
            Multithread action = this.functionsToBeResolved.Dequeue();
            action.FinishMultithread();
        }
    }

    public void AddToThreadPool(Multithread multiThread) {
        functionsToBeRunInThread.Enqueue(multiThread);
    }

    private void RunThread() {
        while (isRunning) { // && !exitHandle.Wait(20)
            if (this.functionsToBeRunInThread.Count > 0) {
                //Thread.Sleep(20);
                Multithread newFunction = this.functionsToBeRunInThread.Dequeue();
                if (newFunction != null) {
                    lock (THREAD_LOCKER) {
                        newFunction.DoMultithread();
                    }
                    this.functionsToBeResolved.Enqueue(newFunction);
                }
            }
        }
        
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
