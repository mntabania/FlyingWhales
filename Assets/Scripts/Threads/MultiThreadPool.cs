using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using Threads;

public class MultiThreadPool : BaseMonoBehaviour {
    public static MultiThreadPool Instance;

    public static readonly object THREAD_LOCKER = new object();

    //private Queue<Multithread> functionsToBeRunInThread;
    private ConcurrentQueue<Multithread> generalFunctionsToBeResolved;
    private ConcurrentQueue<SQLWorkerItem> logFunctionsToBeResolved;

    //private Thread newThread;
    //private ManualResetEventSlim exitHandle = new ManualResetEventSlim();
    //private bool isRunning;

    private Timer timer;

    void Awake() {
        Instance = this;
        //this.isRunning = true;

        //functionsToBeRunInThread = new Queue<Multithread>();
        generalFunctionsToBeResolved = new ConcurrentQueue<Multithread>();
        logFunctionsToBeResolved = new ConcurrentQueue<SQLWorkerItem>();

        //newThread = new Thread(RunThread);
        //newThread.IsBackground = true;
        //newThread.Start();
    }
    protected override void OnDestroy() {
        //this.isRunning = false;
        //functionsToBeRunInThread.Clear();
        generalFunctionsToBeResolved = null;
        logFunctionsToBeResolved = null;
        base.OnDestroy();
        Instance = null;
    }

    void LateUpdate() {
        if (generalFunctionsToBeResolved.Count > 0) {
            Multithread action;
            if (generalFunctionsToBeResolved.TryDequeue(out action)) {
                action.FinishMultithread();
            }
        }
        if (logFunctionsToBeResolved.Count > 0) {
            SQLWorkerItem action;
            if (logFunctionsToBeResolved.TryDequeue(out action)) {
                action.FinishMultithread();
                ObjectPoolManager.Instance.ReturnLogDatabaseThreadToPool(action);
            }
        }
    }

    public void AddToThreadPool(Multithread multiThread) {
        //functionsToBeRunInThread.Enqueue(multiThread);
        //ThreadPool.UnsafeQueueUserWorkItem(ThreadQueueFunction, multiThread);
        ThreadPool.QueueUserWorkItem(ThreadQueueFunction, multiThread);
    }
    private void ThreadQueueFunction(object p_thread) {
        //lock (THREAD_LOCKER) {
        Multithread mt = p_thread as Multithread;
        mt.DoMultithread();
        if (mt is SQLWorkerItem sqlItem) {
            logFunctionsToBeResolved.Enqueue(sqlItem);
        } else {
            generalFunctionsToBeResolved.Enqueue(mt);
        }
        //}
    }
    public bool IsThereStillFunctionsToBeResolved() {
        return generalFunctionsToBeResolved.Count > 0 || logFunctionsToBeResolved.Count > 0;
    }

    //private void RunThread() {
    //    while (isRunning) { // && !exitHandle.Wait(20)
    //        lock (THREAD_LOCKER) {
    //            if (this.functionsToBeRunInThread != null && this.functionsToBeRunInThread.Count > 0) {
    //                //Thread.Sleep(20);
    //                Multithread newFunction = this.functionsToBeRunInThread.Dequeue();
    //                if (newFunction != null) {
    //                    timer = new System.Threading.Timer(TimerCallback, newFunction, 1000, 1000);
    //                    newFunction.DoMultithread();
    //                    elapsedTime = 0;
    //                    timer.Dispose();
    //                    this.functionsToBeResolved.Enqueue(newFunction);
    //                }
    //            }
    //        }
    //    }
    //}

    //private int elapsedTime;
    //private void TimerCallback(object state) {
    //    elapsedTime++;
    //    if (elapsedTime == 10) {
    //        GoapThread goapThread = state as GoapThread;
    //        if (goapThread != null) {
    //            Debug.unityLogger.LogError("Error", $"{goapThread.actor.name}'s GoapThread has exceeded 10 seconds! " +
    //                                                $"\nJob is {(goapThread.job?.jobType.ToString() ?? "None")}" +
    //                                                $"\nTarget is {goapThread.target.name}" +
    //                                                $"\nTarget action is {goapThread.goalType.ToString()}" +
    //                                                $"\nTarget effect is {goapThread.goalEffect.ToString()}");    
    //        }
    //    }
    //}
}
