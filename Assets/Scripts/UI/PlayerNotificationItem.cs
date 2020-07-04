using System;
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerNotificationItem : PooledObject {

    private const int Expiration_Ticks = 10;
    private int ticksAlive = 0;

    public Log shownLog { get; private set; }
    public int tickShown { get; private set; }

    //[SerializeField] private EnvelopContentUnityUI mainEnvelopContent;
    //[SerializeField] private EnvelopContentUnityUI logEnvelopContent;
    [SerializeField] private TextMeshProUGUI logLbl;
    [SerializeField] private LogItem logItem;
    [SerializeField] private RectTransform _container;
    [SerializeField] private LayoutElement _layoutElement;
    private UIHoverPosition _hoverPosition;

    private System.Action<PlayerNotificationItem> onDestroyAction;


    public void Initialize(Log log, bool hasExpiry = true, System.Action<PlayerNotificationItem> onDestroyAction = null) {
        shownLog = log;
        tickShown = GameManager.Instance.Today().tick;
        logLbl.text = $"[{GameManager.ConvertTickToTime(tickShown)}] {UtilityScripts.Utilities.LogReplacer(log)}";
        logItem.SetLog(log);
        //logEnvelopContent.Execute();
        //mainEnvelopContent.Execute();

        //NOTE: THIS IS REMOVED BECAUSE NOTIFICATIONS NO LONGER HAVE TIMERS, INSTEAD THEY WILL JUST BE REPLACED IF NEW ONES ARE ADDED
        //if (hasExpiry) {
        //    //schedule expiry
        //    Messenger.AddListener(Signals.TICK_ENDED, CheckForExpiry);
        //}

        this.onDestroyAction = onDestroyAction;
        StartCoroutine(TweenHeight());
    }
    public void SetHoverPosition(UIHoverPosition hoverPosition) {
        _hoverPosition = hoverPosition;
    }
    IEnumerator TweenHeight() {
        yield return null;
        _layoutElement.DOPreferredSize(new Vector2(0f, (logLbl.transform as RectTransform).sizeDelta.y), 0.5f);
        //_layoutElement.preferredHeight = (logLbl.transform as RectTransform).sizeDelta.y;
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
        _container.anchoredPosition = Vector2.zero;
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
    public void TweenIn() {
        _container.anchoredPosition = new Vector2(450f, 0f);
        _container.DOAnchorPosX(0f, 0.5f);
    }
    
    public void OnHoverOverLog(object obj) {
        if (obj is string indexText) {
            int index = Int32.Parse(indexText);
            LogFiller logFiller = logItem.log.fillers[index];
            if (logFiller.obj is Character character && _hoverPosition != null) {
                UIManager.Instance.ShowCharacterNameplateTooltip(character, _hoverPosition);
            }
        }
    }
    public void OnHoverOutLog() {
        UIManager.Instance.HideCharacterNameplateTooltip();
    }
}
