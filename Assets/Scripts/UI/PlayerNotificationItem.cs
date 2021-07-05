using System;
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Logs;
using Object_Pools;
using UnityEngine.Assertions;

public class PlayerNotificationItem : PooledObject {
    
    public int tickShown { get; private set; }
    public string fromActionID { get; private set; }
    public string logPersistentID { get; private set; }

    private string _involvedObjects;
    
    [SerializeField] private TextMeshProUGUI logLbl;
    [SerializeField] private TextMeshProUGUI dateLbl;
    [SerializeField] private RectTransform _container;
    [SerializeField] private LogsTagButton _logsTagButton;
    [SerializeField] private LayoutElement _layoutElement;
    [SerializeField] private Image _bg;
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _importantSprite;
    [SerializeField] private EventLabel _logEventLbl;
    protected UIHoverPosition _hoverPosition;

    private Action<PlayerNotificationItem> onDestroyAction;
    private bool _adjustHeightOnEnable;

    #region getters
    public string currentTextDisplayed => $"{dateLbl.text} - {logLbl.text}";
    #endregion
    
    private void Awake() {
        _logEventLbl.SetOnRightClickAction(OnRightClickLog);
    }
    private void OnEnable() {
        if (_adjustHeightOnEnable) {
            StartCoroutine(InstantHeight());
            _adjustHeightOnEnable = false;
        }
    }
    public void Initialize(Log log, Action<PlayerNotificationItem> onDestroyAction = null) {
        logPersistentID = log.persistentID;
        tickShown = GameManager.Instance.Today().tick;
        dateLbl.text = log.gameDate.ConvertToTime();    
        logLbl.text = log.logText;
        fromActionID = log.actionID;
        _involvedObjects = log.allInvolvedObjectIDs;
        _bg.sprite = log.IsImportant() ? _importantSprite : _normalSprite;
        this.onDestroyAction = onDestroyAction;
        _logsTagButton.SetTags(log.tags);
        Messenger.AddListener<Log>(UISignals.LOG_REMOVED_FROM_DATABASE, OnLogRemovedFromDatabase);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
    }
    public void Initialize(Log log, int tick, Action<PlayerNotificationItem> onDestroyAction = null) {
        logPersistentID = log.persistentID;
        tickShown = tick;
        dateLbl.text = log.gameDate.ConvertToTime();
        logLbl.text = log.logText;
        fromActionID = log.actionID;
        _involvedObjects = log.allInvolvedObjectIDs;
        _bg.sprite = log.IsImportant() ? _importantSprite : _normalSprite;
        this.onDestroyAction = onDestroyAction;
        _logsTagButton.SetTags(log.tags);
        Messenger.AddListener<Log>(UISignals.LOG_REMOVED_FROM_DATABASE, OnLogRemovedFromDatabase);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
    }
    public void SetHoverPosition(UIHoverPosition hoverPosition) {
        _hoverPosition = hoverPosition;
    }
    public void DoTweenHeight() {
        StartCoroutine(TweenHeight());
    }
    public void QueueAdjustHeightOnEnable() {
        _adjustHeightOnEnable = true;
        // RectTransform thisRect = transform as RectTransform;
        // thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, sizeDelta.y);
    }
    private IEnumerator TweenHeight() {
        yield return null;
        _layoutElement.DOPreferredSize(new Vector2(0f, (logLbl.transform as RectTransform).sizeDelta.y), 0.5f);
        //_layoutElement.preferredHeight = (logLbl.transform as RectTransform).sizeDelta.y;
    }
    private IEnumerator InstantHeight() {
        yield return null;
        RectTransform logRect = logLbl.transform as RectTransform;
        var sizeDelta = logRect.sizeDelta;
        _layoutElement.preferredHeight = sizeDelta.y;
        // _layoutElement.DOPreferredSize(new Vector2(0f, (logLbl.transform as RectTransform).sizeDelta.y), 0.5f);
        //_layoutElement.preferredHeight = (logLbl.transform as RectTransform).sizeDelta.y;
    }
    public void DeleteNotification() {
        onDestroyAction?.Invoke(this);
        ObjectPoolManager.Instance.DestroyObject(this);
    }
    public virtual void DeleteOldestNotification() {
        DeleteNotification();
    }
    public void TweenIn() {
        _container.anchoredPosition = new Vector2(450f, 0f);
        _container.DOAnchorPosX(0f, 0.5f);
    }

    public void OnHoverOverLog(object obj) {
        if (obj is Character character && _hoverPosition != null) {
            Character characterToShow = character;
            if(character.isLycanthrope) {
                characterToShow = character.lycanData.activeForm;
            }
            UIManager.Instance.ShowCharacterNameplateTooltip(characterToShow, _hoverPosition);
        }
    }
    public void OnHoverOutLog() {
        UIManager.Instance.HideCharacterNameplateTooltip();
    }
    private void OnRightClickLog(object obj) {
        if (obj is IPlayerActionTarget playerActionTarget) {
            if (playerActionTarget is Character character) {
                if(character.isLycanthrope) {
                    playerActionTarget = character.lycanData.activeForm;
                }
            }
            UIManager.Instance.ShowPlayerActionContextMenu(playerActionTarget, Input.mousePosition, true);
        }
    }

    #region Listeners
    private void OnLogRemovedFromDatabase(Log log) {
        if (log.persistentID == logPersistentID) {
            // Assert.IsFalse(this is IntelNotificationItem, $"Intel log was removed from database! This should never happen! {logPersistentID}");
            // //if log in this notification is removed from database, then destroy it.
            DeleteNotification();
        }
    }
    private void OnCharacterChangedName(Character character) {
        if (character != null && !string.IsNullOrEmpty(_involvedObjects) && _involvedObjects.Contains(character.persistentID)) {
            Log log = DatabaseManager.Instance.mainSQLDatabase.GetFullLogWithPersistentID(logPersistentID);
            if (log != null) {
                log.TryUpdateLogAfterRename(character);
                logLbl.text = log.logText;
                if (gameObject.activeInHierarchy) {
                    StartCoroutine(InstantHeight());
                }
            }
            LogPool.Release(log);
        }
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        _logsTagButton.Reset();
        _adjustHeightOnEnable = false;
        _container.anchoredPosition = Vector2.zero;
        transform.localScale = Vector3.one;
        fromActionID = string.Empty;
        logPersistentID = string.Empty;
        Messenger.RemoveListener<Log>(UISignals.LOG_REMOVED_FROM_DATABASE, OnLogRemovedFromDatabase);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
    }
    #endregion
}
