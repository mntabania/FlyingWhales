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
    
    private System.Action _onClickAction;
    
    
    public void GeneratePortrait(PortraitSettings portraitSettings) {
        Sprite sprite = CharacterManager.Instance.GetWholeImagePortraitSprite(portraitSettings.wholeImage);
        UpdatePortrait(sprite);
    }
    public void GeneratePortrait(SUMMON_TYPE p_monsterType) {
        Sprite sprite = CharacterManager.Instance.GetSummonSettings(p_monsterType).summonPortrait;
        UpdatePortrait(sprite);
    }
    public void GeneratePortrait(MINION_TYPE p_demonType) {
        Sprite sprite = CharacterManager.Instance.GetMinionSettings(p_demonType).minionPortrait;
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
    #endregion
}
