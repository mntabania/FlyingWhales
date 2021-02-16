﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Ruinarch.Custom_UI;

public abstract class InfoUIBase : MonoBehaviour {
    public Button backButton;
    public bool isShowing;
    private Action openMenuAction;

    protected object _data;

    private IPlayerActionTarget _playerActionTarget;
    
    [Header("Actions")]
    [SerializeField] protected RectTransform actionsTransform;
    [SerializeField] protected GameObject actionItemPrefab;

    private RuinarchToggle[] _toggles;
    
    #region virtuals
    internal virtual void Initialize() {
        Messenger.AddListener<InfoUIBase>(UISignals.BEFORE_MENU_OPENED, BeforeMenuOpens);
        _toggles = GetComponentsInChildren<RuinarchToggle>(true);
    }
    protected void ListenToPlayerActionSignals() {
        Messenger.AddListener<PlayerAction>(SpellSignals.ON_EXECUTE_PLAYER_ACTION, OnExecutePlayerAction);
        Messenger.AddListener<IPlayerActionTarget>(SpellSignals.RELOAD_PLAYER_ACTIONS, ReloadPlayerActions);
        Messenger.AddListener(SpellSignals.FORCE_RELOAD_PLAYER_ACTIONS, ForceReloadPlayerActions);
        Messenger.AddListener<PLAYER_SKILL_TYPE, IPlayerActionTarget>(SpellSignals.PLAYER_ACTION_ADDED_TO_TARGET, OnPlayerActionAddedToTarget);
        Messenger.AddListener<PLAYER_SKILL_TYPE, IPlayerActionTarget>(SpellSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, OnPlayerActionRemovedFromTarget);
    }
    private void OnReceiveHideMenuSignal() {
        if (isShowing) {
            OnClickCloseMenu();
        }
    }
    public virtual void OpenMenu() {
        Messenger.Broadcast(UISignals.BEFORE_MENU_OPENED, this);
        isShowing = true;
        bool wasShowingBefore = gameObject.activeSelf;
        this.gameObject.SetActive(true);
        if (wasShowingBefore) {
            //if ui was showing before, fire show toggle signals of all child toggles, since their OnEnable function will not be called
            //this is so that anything that listens to TOGGLE_SHOWN signal will receive it when this menu's data has changed but has not closed
            if (_toggles != null) {
                for (int i = 0; i < _toggles.Length; i++) {
                    RuinarchToggle toggle = _toggles[i];
                    toggle.FireToggleShownSignal();
                }
            }
        }
        
        if (openMenuAction != null) {
            openMenuAction();
            openMenuAction = null;
        }
        Messenger.Broadcast(UISignals.MENU_OPENED, this);
        
        UIManager.Instance.poiTestingUI.HideUI();
        UIManager.Instance.customDropdownList.Close();
        if(_data is Minion minion) {
            _playerActionTarget = minion.character;
        } else {
            _playerActionTarget = _data as IPlayerActionTarget;
        }
        // if (_playerActionTarget != null) {
        //     LoadActions(_playerActionTarget);    
        // }
        //FactionInfoHubUI.Instance.OnClickClose();
    }
    public virtual void CloseMenu() {
        isShowing = false;
        this.gameObject.SetActive(false);
        Messenger.Broadcast(UISignals.MENU_CLOSED, this);
    }
    public virtual void SetData(object data) {
        _data = data;
    }
    public virtual void ShowTooltip(GameObject objectHovered) {

    }
    protected virtual void OnExecutePlayerAction(PlayerAction action) {
        if (_playerActionTarget != null && _playerActionTarget.actions.Contains(action.type)) {
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
            PlayerAction action = PlayerSkillManager.Instance.GetPlayerActionData(target.actions[i]);
            if (action.IsValid(target) && PlayerManager.Instance.player.playerSkillComponent.CanDoPlayerAction(action.type)) {
                ActionItem actionItem = AddNewAction(action, target);
                actionItem.SetInteractable(action.CanPerformAbilityTo(target) && !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI);    
                actionItem.ForceUpdateCooldown();
            }
        }
    }
    protected ActionItem AddNewAction(PlayerAction playerAction, IPlayerActionTarget target) {
        GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(actionItemPrefab.name, Vector3.zero,
            Quaternion.identity, actionsTransform);
        obj.SetActive(false);
        ActionItem item = obj.GetComponent<ActionItem>();
        item.SetAction(playerAction, target);
        //playerAction.SetActionItem(item);
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
    private void OnPlayerActionAddedToTarget(PLAYER_SKILL_TYPE playerAction, IPlayerActionTarget actionTarget) {
        if (_playerActionTarget == actionTarget && isShowing) {
            LoadActions(actionTarget);
        }
    }
    private void OnPlayerActionRemovedFromTarget(PLAYER_SKILL_TYPE playerAction, IPlayerActionTarget actionTarget) {
        if (_playerActionTarget == actionTarget && isShowing) {
            LoadActions(actionTarget);
        }
    }
    private void ReloadPlayerActions(IPlayerActionTarget actionTarget) {
        if (_playerActionTarget == actionTarget && isShowing) {
            LoadActions(actionTarget);
        }
    }
    private void ForceReloadPlayerActions() {
        if (isShowing && _playerActionTarget != null) {
            LoadActions(_playerActionTarget);
        }
    }
    protected ActionItem GetActiveActionItem(PlayerAction action) {
        for (int i = 0; i < activeActionItems.Count; i++) {
            ActionItem item = activeActionItems[i];
            if (item.playerAction == action) {
                return item;
            }
        }
        return null;
    }
    #endregion
}
