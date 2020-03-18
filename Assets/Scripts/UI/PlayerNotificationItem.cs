using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNotificationItem : PooledObject {

    private const int Expiration_Ticks = 10;
    private int ticksAlive = 0;

    public Log shownLog { get; private set; }
    public int tickShown { get; private set; }

    //[SerializeField] private EnvelopContentUnityUI mainEnvelopContent;
    //[SerializeField] private EnvelopContentUnityUI logEnvelopContent;
    [SerializeField] private TextMeshProUGUI logLbl;
    [SerializeField] private LogItem logItem;

    private System.Action<PlayerNotificationItem> onDestroyAction;


    public void Initialize(Log log, bool hasExpiry = true, System.Action<PlayerNotificationItem> onDestroyAction = null) {
        shownLog = log;
        tickShown = GameManager.Instance.Today().tick;
        logLbl.SetText($"[{GameManager.ConvertTickToTime(tickShown)}] {UtilityScripts.Utilities.LogReplacer(log)}");
        logItem.SetLog(log);
        //logEnvelopContent.Execute();
        //mainEnvelopContent.Execute();

        //NOTE: THIS IS REMOVED BECAUSE NOTIFICATIONS NO LONGER HAVE TIMERS, INSTEAD THEY WILL JUST BE REPLACED IF NEW ONES ARE ADDED
        //if (hasExpiry) {
        //    //schedule expiry
        //    Messenger.AddListener(Signals.TICK_ENDED, CheckForExpiry);
        //}

        this.onDestroyAction = onDestroyAction;
    }
    public void SetTickShown(int tick) {
        tickShown = tick;
        logLbl.SetText($"[{GameManager.ConvertTickToTime(tickShown)}] {UtilityScripts.Utilities.LogReplacer(shownLog)}");
    }
    private void CheckForExpiry() {
        if (ticksAlive == Expiration_Ticks) {
            DeleteNotification();
        } else {
            ticksAlive++;
        }
    }
    protected virtual void OnExpire() {
        DeleteNotification();
        //getIntelBtn.interactable = false;
    }
    public override void Reset() {
        base.Reset();
        //getIntelBtn.interactable = true;
        ticksAlive = 0;
        this.transform.localScale = Vector3.one;
    }
    public void DeleteNotification() {
        //if (Messenger.eventTable.ContainsKey(Signals.TICK_ENDED)) {
        //    Messenger.RemoveListener(Signals.TICK_ENDED, CheckForExpiry);
        //}
        onDestroyAction?.Invoke(this);
        ObjectPoolManager.Instance.DestroyObject(this);
    }
}
