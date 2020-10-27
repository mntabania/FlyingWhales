using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MultiThreadPool : BaseMonoBehaviour {
    public static MultiThreadPool Instance;

    private static readonly object THREAD_LOCKER = new object();

    private Queue<Multithread> functionsToBeRunInThread;
    private Queue<Multithread> functionsToBeResolved;

    private Thread newThread;
    //private ManualResetEventSlim exitHandle = new ManualResetEventSlim();
    private bool isRunning;

    private Timer timer;

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
                    timer = new System.Threading.Timer(TimerCallback, newFunction, 1000, 1000);
                    lock (THREAD_LOCKER) {
                        newFunction.DoMultithread();
                    }
                    elapsedTime = 0;
                    timer.Dispose();
                    this.functionsToBeResolved.Enqueue(newFunction);
                }
            }
        }
    }

    private int elapsedTime;
    private void TimerCallback(object state) {
        elapsedTime++;
        if (elapsedTime == 10) {
            GoapThread goapThread = state as GoapThread;
            if (goapThread != null) {
                Debug.unityLogger.LogError("Error", $"{goapThread.actor.name}'s GoapThread has exceeded 10 seconds! " +
                                                    $"\nJob is {(goapThread.job?.jobType.ToString() ?? "None")}" +
                                                    $"\nTarget is {goapThread.target.name}" +
                                                    $"\nTarget action is {goapThread.goalType.ToString()}" +
                                                    $"\nTarget effect is {goapThread.goalEffect.ToString()}");    
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
