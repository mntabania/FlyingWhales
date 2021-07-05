using System;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseCharacterPortrait : PooledObject, IPointerClickHandler {
    [Header("BG")]
    [SerializeField] private Image baseBG;

    [Header("Face")]
    [SerializeField] private Image wholeImage;

    [Header("Other")]
    [SerializeField] private GameObject hoverObj;
    [SerializeField] private bool ignoreInteractions = false;
    [SerializeField] private HoverHandler hoverHandler;
    
    private System.Action _onClickAction;
    private System.Action _onHoverOverAction;
    private System.Action _onHoverOutAction;
    private void Awake() {
        hoverHandler.AddOnHoverOverAction(OnHoverOver);
        hoverHandler.AddOnHoverOutAction(OnHoverOut);
    }
    public void GeneratePortrait(PortraitSettings portraitSettings) {
        Sprite sprite = CharacterManager.Instance.GetOrCreateCharacterClassData(portraitSettings.className)?.portraitSprite;
        UpdatePortrait(sprite);
    }
    public void GeneratePortrait(SUMMON_TYPE p_monsterType) {
        Sprite sprite = CharacterManager.Instance.GetOrCreateCharacterClassData(CharacterManager.Instance.GetSummonSettings(p_monsterType).className).portraitSprite;
        UpdatePortrait(sprite);
    }
    public void GeneratePortrait(MINION_TYPE p_demonType) {
        Sprite sprite = CharacterManager.Instance.GetOrCreateCharacterClassData(CharacterManager.Instance.GetMinionSettings(p_demonType).className).portraitSprite;
        UpdatePortrait(sprite);
    }
    
     private void UpdatePortrait(Sprite p_sprite) {
        if (p_sprite != null) {
            //use portrait sprite directly
            SetWholeImageSprite(p_sprite);
            SetWholeImageState(true);
        } else {
            //use whole image
            SetWholeImageSprite(null);
            SetWholeImageState(false);
        }
     }
     
     private void SetWholeImageState(bool state) {
         wholeImage.gameObject.SetActive(state);
     }
     private void SetWholeImageSprite(Sprite sprite) {
         wholeImage.sprite = sprite;
     }
     
    #region Pointer Actions
    public void AddPointerClickAction(System.Action p_action) {
        _onClickAction += p_action;
    }
    public void OnPointerClick(PointerEventData eventData) {
        if (ignoreInteractions) {
            return;
        }
        if (eventData.button == PointerEventData.InputButton.Left) {
            OnLeftClick();
        } else if (eventData.button == PointerEventData.InputButton.Right) {
            OnRightClick();
        }
    }
    public void OnClick(BaseEventData eventData) {
        if (ignoreInteractions || !gameObject.activeSelf) {
            return;
        }
        OnPointerClick(eventData as PointerEventData);
    } 
    public void OnLeftClick() { 
        _onClickAction?.Invoke();
    }
    private void OnRightClick() { }
    public void SetHoverHighlightState(bool state) { 
        hoverObj.SetActive(state);
    }
    private void OnHoverOver() {
        _onHoverOverAction?.Invoke();
    }
    private void OnHoverOut() {
        _onHoverOutAction?.Invoke();
    }
    public void AddHoverOverAction(System.Action p_action) {
        _onHoverOverAction += p_action;
    }
    public void AddHoverOutAction(System.Action p_action) {
        _onHoverOutAction += p_action;
    }
    public void RemoveHoverOverAction(System.Action p_action) {
        _onHoverOverAction -= p_action;
    }
    public void RemoveHoverOutAction(System.Action p_action) {
        _onHoverOutAction -= p_action;
    }
    #endregion
}
