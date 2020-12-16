using System;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoodThoughtUIItem : PooledObject {
    [SerializeField] private TextMeshProUGUI txtThought;
    [SerializeField] private TextMeshProUGUI txtMoodModificationAmount;
    [SerializeField] private Image imgBG;
    [SerializeField] private Sprite spritePositive;
    [SerializeField] private Sprite spriteNegative;
    [SerializeField] private Sprite spriteNeutral;
    [SerializeField] private HoverHandler hoverHandler;
    [SerializeField] private EventLabel eventLabelThoughts;

    private GameDate _expiryDate;
    private System.Action<GameDate> _onHoverOverItem;
    private System.Action _onHoverOutItem;
    private void OnEnable() {
        hoverHandler.AddOnHoverOverAction(OnHoverOverItem);
        hoverHandler.AddOnHoverOutAction(OnHoverOutItem);
        eventLabelThoughts.SetOnRightClickAction(OnRightClickObjectInLog);
    }
    private void OnDisable() {
        hoverHandler.RemoveOnHoverOverAction(OnHoverOverItem);
        hoverHandler.RemoveOnHoverOutAction(OnHoverOutItem);
    }
    public void SetItemDetails(string p_thought, int p_amount, GameDate p_expiryDate, System.Action<GameDate> p_onHoverOverItem, System.Action p_onHoverOutItem) {
        txtThought.text = p_thought;
        _expiryDate = p_expiryDate;
        if (p_amount > 0) {
            imgBG.sprite = spritePositive;
            txtMoodModificationAmount.text = $"<color=\"green\">+{p_amount.ToString()}</color>";  
        } else if (p_amount < 0) {
            imgBG.sprite = spriteNegative;
            txtMoodModificationAmount.text = $"<color=\"red\">{p_amount.ToString()}</color>";
        } else {
            imgBG.sprite = spriteNeutral;
            txtMoodModificationAmount.text = p_amount.ToString();    
        }
        _onHoverOverItem = p_onHoverOverItem;
        _onHoverOutItem = p_onHoverOutItem;
    }

    private void OnHoverOverItem() {
        _onHoverOverItem?.Invoke(_expiryDate);    
    }
    private void OnHoverOutItem() {
        _onHoverOutItem.Invoke();    
    }
    private void OnRightClickObjectInLog(object obj) {
        if (obj is IPlayerActionTarget playerActionTarget) {
            if (playerActionTarget is Character character) {
                if(character.isLycanthrope) {
                    playerActionTarget = character.lycanData.activeForm;
                }
            }
            UIManager.Instance.ShowPlayerActionContextMenu(playerActionTarget, Input.mousePosition, true);
        }
    }
}
