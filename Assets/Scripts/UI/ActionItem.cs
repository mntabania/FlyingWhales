using System.Collections;
using System.Collections.Generic;
using Actionables;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionItem : PooledObject {

	public PlayerAction playerAction { get; private set; }
	
	[SerializeField] private Button button;
	[SerializeField] private Image actionImg;
	[SerializeField] private Image coverImg;
    [SerializeField] private Image highlightImg;
    [SerializeField] private TextMeshProUGUI actionLbl;

	private string expiryKey;
	
	public void SetAction(PlayerAction playerAction) {
        this.playerAction = playerAction;
        UnToggleHighlight();
        //if (playerAction.actions != null) {
        //    button.onClick.AddListener(playerAction.Execute);
        //}
        // if (icon != null) {
        // 	actionImg.sprite = icon;	
        // }
        actionImg.sprite = PlayerUI.Instance.playerActionIconDictionary[playerAction.actionName];
        actionLbl.text = playerAction.labelText;
		SetAsClickable();
        // Messenger.AddListener<PlayerAction>(Signals.PLAYER_ACTION_UNTOGGLE, ListenUntoggleHighlight);
	}
	public void SetAsUninteractableUntil(int ticks) {
		GameDate date = GameManager.Instance.Today();
		date = date.AddTicks(ticks);
		SetAsUninteractableUntil(date);
	}
	public void SetAsUninteractableUntil(GameDate date) {
        SetInteractable(false);
        expiryKey = SchedulingManager.Instance.AddEntry(date, SetAsClickable, this);
	}
	private void SetAsClickable() {
        SetInteractable(true);
	}
    public void SetInteractable(bool state) {
        button.interactable = state;
        coverImg.gameObject.SetActive(!state);
    }
    public void ToggleHighlight() {
        //if (!playerAction.isInstant) {
            highlightImg.gameObject.SetActive(true);
            // UpdateState();
        //}
    }
    public void UnToggleHighlight() {
        //if (!playerAction.isInstant) {
            highlightImg.gameObject.SetActive(false);
            // UpdateState();
        //}
    }
    private void UpdateState() {
        SetInteractable(playerAction.isActionClickableChecker.Invoke());
    }
    public void OnClickThis() {
        if (playerAction != null && playerAction.actions != null) {
            ToggleHighlight();
            playerAction.Execute();
        }
    }

    #region Listeners
    private void ListenUntoggleHighlight(PlayerAction action) {
        if(action == playerAction) {
            UnToggleHighlight();
        }
    }
    #endregion

    public override void Reset() {
		base.Reset();
		button.onClick.RemoveAllListeners();
		if (string.IsNullOrEmpty(expiryKey) == false) {
			SchedulingManager.Instance.RemoveSpecificEntry(expiryKey);
		}
		expiryKey = string.Empty;
	}
}
