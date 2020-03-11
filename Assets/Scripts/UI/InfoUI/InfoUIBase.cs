using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Actionables;

public abstract class InfoUIBase : MonoBehaviour {
    public Button backButton;
    public bool isShowing;
    private Action openMenuAction;

    protected object _data;

    private IPlayerActionTarget _playerActionTarget;
    
    [Header("Actions")]
    [SerializeField] protected RectTransform actionsTransform;
    [SerializeField] protected GameObject actionItemPrefab;

    #region virtuals
    internal virtual void Initialize() {
        Messenger.AddListener<InfoUIBase>(Signals.BEFORE_MENU_OPENED, BeforeMenuOpens);
        Messenger.AddListener<PlayerAction>(Signals.PLAYER_ACTION_EXECUTED, OnPlayerActionExecuted);
        Messenger.AddListener<IPlayerActionTarget>(Signals.RELOAD_PLAYER_ACTIONS, ReloadPlayerActions);
        Messenger.AddListener<PlayerAction, IPlayerActionTarget>(Signals.PLAYER_ACTION_ADDED_TO_TARGET, OnPlayerActionAddedToTarget);
        Messenger.AddListener<PlayerAction, IPlayerActionTarget>(Signals.PLAYER_ACTION_REMOVED_FROM_TARGET, OnPlayerActionRemovedFromTarget);
        Messenger.AddListener(Signals.HIDE_MENUS, OnReceiveHideMenuSignal);
    }
    private void OnReceiveHideMenuSignal() {
        if (isShowing) {
            OnClickCloseMenu();
        }
    }
    public virtual void OpenMenu() {
        Messenger.Broadcast(Signals.BEFORE_MENU_OPENED, this);
        isShowing = true;
        this.gameObject.SetActive(true);
        if (openMenuAction != null) {
            openMenuAction();
            openMenuAction = null;
        }
        Messenger.Broadcast(Signals.MENU_OPENED, this);
        
        UIManager.Instance.poiTestingUI.HideUI();
        UIManager.Instance.minionCommandsUI.HideUI();
        UIManager.Instance.customDropdownList.Close();
        _playerActionTarget = _data as IPlayerActionTarget;
        if (_playerActionTarget != null) {
            LoadActions(_playerActionTarget);    
        }
    }
    public virtual void CloseMenu() {
        isShowing = false;
        this.gameObject.SetActive(false);
        Messenger.Broadcast(Signals.MENU_CLOSED, this);
    }
    public virtual void SetData(object data) {
        _data = data;
    }
    public virtual void ShowTooltip(GameObject objectHovered) {

    }
    protected virtual void OnPlayerActionExecuted(PlayerAction action) {
        if (_playerActionTarget != null && _playerActionTarget.actions.Contains(action)) {
            LoadActions(_playerActionTarget);
        }
    }
    #endregion

    public void OnClickCloseMenu() {
        CloseMenu();
    }
    private void BeforeMenuOpens(InfoUIBase baseToOpen) {
        if (this.isShowing && baseToOpen != this) {
            CloseMenu();
        }
    }

    #region Actions
    protected List<ActionItem> activeActionItems = new List<ActionItem>();
    protected virtual void LoadActions(IPlayerActionTarget target) {
        UtilityScripts.Utilities.DestroyChildren(actionsTransform);
        activeActionItems.Clear();
        for (int i = 0; i < target.actions.Count; i++) {
            PlayerAction action = target.actions[i];
            if (action.IsValid(target) && PlayerManager.Instance.player.archetype.CanDoAction(action.actionName)) {
                ActionItem actionItem = AddNewAction(action);
                actionItem.SetInteractable(action.isActionClickableChecker.Invoke() && !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI);
            }
        }
    }
    protected ActionItem AddNewAction(PlayerAction playerAction) {
        GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(actionItemPrefab.name, Vector3.zero,
            Quaternion.identity, actionsTransform);
        ActionItem item = obj.GetComponent<ActionItem>();
        item.SetAction(playerAction);
        playerAction.SetActionItem(item);
        activeActionItems.Add(item);
        return item;
    }
    private ActionItem GetActionItem(PlayerAction action) {
        for (int i = 0; i < activeActionItems.Count; i++) {
            ActionItem item = activeActionItems[i];
            if (item.playerAction == action) {
                return item;
            }
        }
        return null;
    }
    private void OnPlayerActionAddedToTarget(PlayerAction playerAction, IPlayerActionTarget actionTarget) {
        if (_playerActionTarget == actionTarget && isShowing) {
            LoadActions(actionTarget);
        }
    }
    private void OnPlayerActionRemovedFromTarget(PlayerAction playerAction, IPlayerActionTarget actionTarget) {
        if (_playerActionTarget == actionTarget && isShowing) {
            LoadActions(actionTarget);
        }
    }
    private void ReloadPlayerActions(IPlayerActionTarget actionTarget) {
        if (_playerActionTarget == actionTarget && isShowing) {
            LoadActions(actionTarget);
        }
    }
    #endregion
}
